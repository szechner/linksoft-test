using Microsoft.Extensions.Logging;
using SocialNetworkAnalyzer.Core.Abstractions.Utils;
using SocialNetworkAnalyzer.Data.EntityFramework.Base;
using SocialNetworkAnalyzer.Data.EntityFramework.Contexts;
using SocialNetworkAnalyzer.Data.Model.Database;
using SocialNetworkAnalyzer.Data.Repositories;

namespace SocialNetworkAnalyzer.Data.EntityFramework;

/// <summary>
/// EF Core Implementation of <see cref="IDataSetsRepository"/>
/// </summary>
/// <param name="dbContext"></param>
/// <param name="logger"></param>
public class DataSetsRepository(SocialMappingContext dbContext, ILogger<DataSetsRepository> logger) : EfRepositoryBase<DataSet>(dbContext, logger), IDataSetsRepository
{
    /// <inheritdoc />
    public async Task<DataSet> CreateDataSet(string name, CancellationToken cancellationToken)
    {
        using var _ = Measurement.ElapsedTime(this);
        cancellationToken.ThrowIfCancellationRequested();

        var dataSet = new DataSet
        {
            Name = name
        };

        await DbContext.AddAsync(dataSet, cancellationToken);
        await DbContext.SaveChangesAsync(cancellationToken);

        Logger.LogInformation("Created data set {DataSetId} with name {DataSetName}", dataSet.Id, dataSet.Name);

        return dataSet;
    }
}