using System.Collections.Immutable;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SocialNetworkAnalyzer.Core.Abstractions.Utils;
using SocialNetworkAnalyzer.Core.Guards;
using SocialNetworkAnalyzer.Data.Abstraction;
using SocialNetworkAnalyzer.Data.EntityFramework.Base;
using SocialNetworkAnalyzer.Data.EntityFramework.Contexts;
using SocialNetworkAnalyzer.Data.Model.Database;
using SocialNetworkAnalyzer.Data.Repositories;

namespace SocialNetworkAnalyzer.Data.EntityFramework;

/// <summary>
/// EF Core Implementation of <see cref="IUsersRepository"/>
/// </summary>
public class UsersRepository(SocialMappingContext dbContext, ILogger<UsersRepository> logger) : EfRepositoryBase<User>(dbContext, logger), IUsersRepository
{
    private readonly string tableName = Guard.Require.ArgumentNotNull(dbContext.Model.FindEntityType(typeof(User))?.GetSchemaQualifiedTableName(), nameof(tableName));
    private const int batchSize = 100;

    /// <inheritdoc />
    public async Task Add(ImmutableArray<int> buffer, CancellationToken cancellationToken)
    {
        using var _ = Measurement.ElapsedTime(this);
        
        cancellationToken.ThrowIfCancellationRequested();

        if (buffer.Length == 0)
        {
            logger.LogInformation("Buffer is empty, skipping adding users to {TableName}", tableName);
            return;
        }

        var currentIndex = 0;

        while (currentIndex < buffer.Length)
        {
            var currentBuffer = buffer.Skip(currentIndex).Take(100).ToList();
            // Create bulk insert query
            var sql = new StringBuilder();
            sql.AppendLine($"lock \"{tableName}\" in ACCESS EXCLUSIVE MODE;");
            sql.AppendLine($"insert into \"{tableName}\" (Id)");
            sql.AppendLine("select * from (");
            sql.AppendLine($"select unnest(array[{string.Join(',', currentBuffer.Distinct().Select(t => t.ToString()))}]) as Id");
            sql.AppendLine(") as tmp");
            sql.AppendLine("where not exists (");
            sql.AppendLine($"select 1 from \"{tableName}\" urs where urs.Id = tmp.Id");
            sql.AppendLine(");");

            var affected = await DbContext.Database.ExecuteSqlRawAsync(sql.ToString(), cancellationToken);

            // Commit required to release table lock
            ((ITransactionManager)DbContext).Commit();

            logger.LogInformation("Added {Count} users to {TableName} from buffer {BufferCount}", affected, tableName, buffer.Length - currentIndex);
            currentIndex += batchSize;
        }
    }
}