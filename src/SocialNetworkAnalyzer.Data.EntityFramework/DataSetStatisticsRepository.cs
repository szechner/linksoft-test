using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SocialNetworkAnalyzer.Core.Abstractions.Utils;
using SocialNetworkAnalyzer.Core.Guards;
using SocialNetworkAnalyzer.Data.EntityFramework.Base;
using SocialNetworkAnalyzer.Data.EntityFramework.Contexts;
using SocialNetworkAnalyzer.Data.Model.Database;
using SocialNetworkAnalyzer.Data.Repositories;

namespace SocialNetworkAnalyzer.Data.EntityFramework;

/// <summary>
/// EF Core Implementation of <see cref="IDataSetStatisticsRepository"/>
/// </summary>
public class DataSetStatisticsRepository(SocialMappingContext dbContext, ILogger<DataSetStatisticsRepository> logger) : EfRepositoryBase<DataSetStatistics>(dbContext, logger), IDataSetStatisticsRepository
{
    /// <inheritdoc />
    public async Task<DataSetStatistics> CreateDataSetStatistics(int dataSetId, CancellationToken cancellationToken)
    {
        using var _ = Measurement.ElapsedTime(this);
        
        cancellationToken.ThrowIfCancellationRequested();

        await EnsureDataSetExists(dataSetId, cancellationToken);

        var datasetStatistics = new DataSetStatistics
        {
            DataSetId = dataSetId,
            State = DataSetStatisticsState.Pending
        };

        await DbContext.AddAsync(datasetStatistics, cancellationToken);
        await DbContext.SaveChangesAsync(cancellationToken);

        Logger.LogInformation("Created data set statistics for dataSetId={DataSetId}", dataSetId);

        return datasetStatistics;
    }

    /// <inheritdoc />
    public async Task UpdateNodesCount(int dataSetId, int nodesCount, CancellationToken cancellationToken)
    {
        using var _ = Measurement.ElapsedTime(this);
        
        cancellationToken.ThrowIfCancellationRequested();

        await EnsureDataSetExists(dataSetId, cancellationToken);
        var datasetStatistics = await GetDataSetStatistics(dataSetId, cancellationToken);

        datasetStatistics.NodesCount = nodesCount;
        UpdateState(datasetStatistics);
        await DbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task UpdateRelationshipsCount(int dataSetId, int relationshipsCount, CancellationToken cancellationToken)
    {
        using var _ = Measurement.ElapsedTime(this);
        
        cancellationToken.ThrowIfCancellationRequested();

        await EnsureDataSetExists(dataSetId, cancellationToken);
        var datasetStatistics = await GetDataSetStatistics(dataSetId, cancellationToken);

        datasetStatistics.RelationshipsCount = relationshipsCount;
        UpdateState(datasetStatistics);
        await DbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task UpdateAvgRelationsCount(int dataSetId, double avgRelationsCount, CancellationToken cancellationToken)
    {
        using var _ = Measurement.ElapsedTime(this);
        
        cancellationToken.ThrowIfCancellationRequested();

        await EnsureDataSetExists(dataSetId, cancellationToken);
        var datasetStatistics = await GetDataSetStatistics(dataSetId, cancellationToken);

        datasetStatistics.AvgRelationsCount = avgRelationsCount;
        UpdateState(datasetStatistics);
        await DbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAvgGroupRelationsCount(int dataSetId, double avgGroupRelationsCount, CancellationToken cancellationToken)
    {
        using var _ = Measurement.ElapsedTime(this);
        
        cancellationToken.ThrowIfCancellationRequested();

        await EnsureDataSetExists(dataSetId, cancellationToken);
        var datasetStatistics = await GetDataSetStatistics(dataSetId, cancellationToken);

        datasetStatistics.AvgGroupRelationsCount = avgGroupRelationsCount;
        UpdateState(datasetStatistics);
        await DbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task SetError(int dataSetId, string error, CancellationToken cancellationToken)
    {
        using var _ = Measurement.ElapsedTime(this);
        
        cancellationToken.ThrowIfCancellationRequested();

        await EnsureDataSetExists(dataSetId, cancellationToken);
        var datasetStatistics = await GetDataSetStatistics(dataSetId, cancellationToken);

        datasetStatistics.Error = error;
        datasetStatistics.State = DataSetStatisticsState.Error;

        await DbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<int> CountRows(CancellationToken cancellationToken)
    {
        using var _ = Measurement.ElapsedTime(this);
        
        cancellationToken.ThrowIfCancellationRequested();

        return await DbSet.CountAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<DataSetStatistics>> GetDataSetStatistics(int top, int skip, CancellationToken cancellationToken)
    {
        using var _ = Measurement.ElapsedTime(this);
        
        cancellationToken.ThrowIfCancellationRequested();

        return await DbSet.Include(t => t.DataSet).OrderBy(t => t.DataSetId).Skip(skip).Take(top).ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Ensure that the DataSet with the given id exists 
    /// </summary>
    private async Task EnsureDataSetExists(int dataSetId, CancellationToken cancellationToken)
    {
        using var __ = Measurement.ElapsedTime(this);
        
        cancellationToken.ThrowIfCancellationRequested();

        _ = Guard.Require.ArgumentNotNull(await DbContext.Set<DataSet>().FindAsync([dataSetId], cancellationToken), nameof(dataSetId), $"DataSet with id {dataSetId} not found");
    }

    /// <summary>
    /// Get DataSetStatistics by id of DataSet
    /// </summary>
    private async Task<DataSetStatistics> GetDataSetStatistics(int dataSetId, CancellationToken cancellationToken)
    {
        using var _ = Measurement.ElapsedTime(this);
        
        cancellationToken.ThrowIfCancellationRequested();

        var result = await DbContext.Set<DataSetStatistics>().FindAsync([dataSetId], cancellationToken);

        if (result == null)
        {
            throw new InvalidOperationException($"DataSetStatistics with id {dataSetId} not found");
        }

        return result;
    }

    /// <summary>
    /// Update the state of the DataSetStatistics if all required fields are set 
    /// </summary>
    private static void UpdateState(DataSetStatistics dataSetStatistics)
    {
        if (
            dataSetStatistics.State != DataSetStatisticsState.Calculated &&
            dataSetStatistics.State != DataSetStatisticsState.Error &&
            dataSetStatistics is { NodesCount: not null, RelationshipsCount: not null, AvgRelationsCount: not null, AvgGroupRelationsCount: not null })
        {
            dataSetStatistics.State = DataSetStatisticsState.Calculated;
        }
    }
}