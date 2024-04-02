using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Text;
using FluentValidation;
using FluentValidation.Results;
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
/// EF Core Implementation of <see cref="IRelationshipsRepository"/>
/// </summary>
public class RelationshipsRepository(SocialMappingContext dbContext, ILogger<RelationshipsRepository> logger) : EfRepositoryBase<Relationship>(dbContext, logger), IRelationshipsRepository
{
    private readonly string tableName = Guard.Require.ArgumentNotNull(dbContext.Model.FindEntityType(typeof(Relationship))?.GetSchemaQualifiedTableName(), nameof(tableName));
    private const int batchSize = 1000;

    /// <inheritdoc />
    public async Task Add(ImmutableArray<(int UserId1, int UserId2)> buffer, int dataSetId, CancellationToken cancellationToken)
    {
        using var __ = Measurement.ElapsedTime(this);

        cancellationToken.ThrowIfCancellationRequested();

        _ = Guard.Require.ArgumentNotNull(await DbContext.Set<DataSet>().FindAsync([dataSetId], cancellationToken), nameof(dataSetId), $"DataSet with id {dataSetId} not found");

        if (buffer.Length == 0)
        {
            logger.LogInformation("Buffer is empty, skipping adding users to {TableName}", tableName);
            return;
        }

        var currentIndex = 0;

        if (buffer.Any(t => t.UserId1.Equals(t.UserId2)))
        {
            var badRelations = buffer.Where(t => t.UserId1.Equals(t.UserId2)).Select(t => $"{t.UserId1} -> {t.UserId2}");

            throw new ValidationException(new[]
            {
                new ValidationFailure("Batch", $"User cannot have relation with itself:{Environment.NewLine}{string.Join(Environment.NewLine, badRelations)}")
            });
        }

        while (currentIndex < buffer.Length)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var currentBuffer = buffer.Skip(currentIndex).Take(batchSize).ToList();
            // Create bulk insert query
            var sql = new StringBuilder();
            sql.AppendLine($"lock \"{tableName}\" in ACCESS EXCLUSIVE MODE;");
            sql.AppendLine($"insert into \"{tableName}\" (user_id1,user_id2,data_set_id)");
            sql.AppendLine("select * from (");
            sql.AppendLine("select");
            sql.AppendLine($"unnest(array[{string.Join(',', currentBuffer.Distinct().Select(t => t.UserId1.ToString()))}]) as user_id1,");
            sql.AppendLine($"unnest(array[{string.Join(',', currentBuffer.Distinct().Select(t => t.UserId2.ToString()))}]) as user_id2,");
            sql.AppendLine($"unnest(array[{string.Join(',', currentBuffer.Distinct().Select(_ => dataSetId.ToString()))}]) as dataSetId");
            sql.AppendLine(") as tmp");
            sql.AppendLine("where not exists (");
            sql.AppendLine($"select 1 from \"{tableName}\" rels where rels.user_id1 = tmp.user_id1 and rels.user_id2 = tmp.user_id2 and rels.data_set_id = {dataSetId}");
            sql.AppendLine(");");

            var affected = await DbContext.Database.ExecuteSqlRawAsync(sql.ToString(), cancellationToken);

            // Commit required to release table lock
            ((ITransactionManager)DbContext).Commit();

            logger.LogInformation("Added {Count} relations to {TableName} from buffer {BufferCount}", affected, tableName, buffer.Length - currentIndex);
            currentIndex += batchSize;
        }
    }

    /// <inheritdoc />
    public async Task<int> GetRelationshipsCount(int dataSetId, CancellationToken cancellationToken)
    {
        using var _ = Measurement.ElapsedTime(this);

        cancellationToken.ThrowIfCancellationRequested();

        var sourceUsers1 = DbSet.Where(x => x.DataSetId == dataSetId).Select(t => new { t.UserId1, t.UserId2 });
        var sourceUsers2 = DbSet.Where(x => x.DataSetId == dataSetId).Select(t => new { UserId1 = t.UserId2, UserId2 = t.UserId1 });

        var result = await sourceUsers2.Union(sourceUsers1).Distinct().CountAsync(cancellationToken);

        return result;
    }

    /// <inheritdoc />
    public async Task<int> GetUniqueUsersCount(int dataSetId, CancellationToken cancellationToken)
    {
        using var _ = Measurement.ElapsedTime(this);

        cancellationToken.ThrowIfCancellationRequested();

        var sourceUsers1 = DbSet.Where(x => x.DataSetId == dataSetId).Select(t => t.UserId1).Distinct();
        var sourceUsers2 = DbSet.Where(x => x.DataSetId == dataSetId).Select(t => t.UserId2).Distinct();

        var result = await sourceUsers2.Union(sourceUsers1).Distinct().CountAsync(cancellationToken);
        return result;
    }

    /// <inheritdoc />
    public async Task<double> GetAvgRelationsCount(int dataSetId, CancellationToken cancellationToken)
    {
        using var _ = Measurement.ElapsedTime(this);

        cancellationToken.ThrowIfCancellationRequested();

        var sourceUsers1 = DbSet.Where(x => x.DataSetId == dataSetId).Select(t => new { t.UserId1, t.UserId2 });
        var sourceUsers2 = DbSet.Where(x => x.DataSetId == dataSetId).Select(t => new { UserId1 = t.UserId2, UserId2 = t.UserId1 });

        var source = sourceUsers2.Union(sourceUsers1).Distinct();

        var groupUsers1 = source.GroupBy(x => x.UserId1).Select(x => x.Count());
        var groupUsers2 = source.GroupBy(x => x.UserId2).Select(x => x.Count());

        var groupUsers = await groupUsers1.Concat(groupUsers2).Distinct().ToListAsync(cancellationToken);

        var result = groupUsers.Any() ? groupUsers.Average() : 0;
        return double.Round(result, 2);
    }

    /// <inheritdoc />
    public async Task<double> GetAvgGroupRelationsCount(int dataSetId, CancellationToken cancellationToken)
    {
        using var _ = Measurement.ElapsedTime(this);

        cancellationToken.ThrowIfCancellationRequested();

        var sourceUsers1 = DbSet.Where(x => x.DataSetId == dataSetId).Select(t => new { t.UserId1, t.UserId2 });
        var sourceUsers2 = DbSet.Where(x => x.DataSetId == dataSetId).Select(t => new { UserId1 = t.UserId2, UserId2 = t.UserId1 });

        var source = await sourceUsers2.Union(sourceUsers1).Where(t => t.UserId1 != t.UserId2).Distinct().ToListAsync(cancellationToken);

        var allUsers = source.Select(t => t.UserId1).Union(source.Select(t => t.UserId2)).Distinct().ToList();

        var dynamicSource = new ConcurrentBag<dynamic>(source.ToList<dynamic>());

        var groups = new ConcurrentBag<string>();
        
        Parallel.ForEach(allUsers, user =>
        {
            var groupConnection = GetGroupConnections(user, dynamicSource);
            if (groupConnection == null) return;
            
            var groupString = string.Join(",", groupConnection.Order());
            if (!groups.Contains(groupString))
            {
                groups.Add(groupString);
            }
        });

        var distinctGroups = groups.Distinct().Select(t => t.Split(","));

        var result =  distinctGroups.Any() ? distinctGroups.Average(t => t.Length) : 0;
        return double.Round(result,2);
    }

    private static List<int>? GetGroupConnections(int userId, ConcurrentBag<dynamic> source)
    {
        var result = new List<int>();

        var connections = source.Where(t => t.UserId1 == userId).Select(t => (int)t.UserId2).ToList();
        result.Add(userId);
        result.AddRange(connections);

        foreach (var connection in connections)
        {
            var userId2Connections = source.Where(t => t.UserId1 == connection).Select(t => (int)t.UserId2).ToList();
            userId2Connections.Add(connection);

            result = result.Intersect(userId2Connections.ToList()).ToList();
        }

        return result.Count <= 1 ? null : result.Distinct().ToList();
    }
}