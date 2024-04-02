using SocialNetworkAnalyzer.Data.Abstraction;
using SocialNetworkAnalyzer.Data.Model.Database;

namespace SocialNetworkAnalyzer.Data.Repositories;

/// <summary>
/// Repository for <see cref="DataSetStatistics"/>
/// </summary>
public interface IDataSetStatisticsRepository : IDataRepository<DataSetStatistics>
{
    /// <summary>
    /// Create statistics for the dataset in database 
    /// </summary>
    Task<DataSetStatistics> CreateDataSetStatistics(int dataSetId, CancellationToken cancellationToken);

    /// <summary>
    /// Update the number of nodes in the dataset statistics 
    /// </summary>
    Task UpdateNodesCount(int dataSetId, int nodesCount, CancellationToken cancellationToken);
    
    /// <summary>
    /// Update the number of relationships in the dataset statistics 
    /// </summary>
    Task UpdateRelationshipsCount(int dataSetId, int relationshipsCount, CancellationToken cancellationToken);
    
    /// <summary>
    /// Update the number of average user's relations in the dataset statistics 
    /// </summary>
    Task UpdateAvgRelationsCount(int dataSetId, double avgRelationsCount, CancellationToken cancellationToken);
    
    /// <summary>
    /// Update the number of average groups of users relations in the dataset statistics 
    /// </summary>
    Task UpdateAvgGroupRelationsCount(int dataSetId, double avgGroupRelationsCount, CancellationToken cancellationToken);
    
    /// <summary>
    /// Store an error in the dataset statistics and set the state to Error
    /// </summary>
    Task SetError(int dataSetId, string error, CancellationToken cancellationToken);
    
    /// <summary>
    /// Returns the number of rows in the table 
    /// </summary>
    Task<int> CountRows(CancellationToken cancellationToken);
    
    /// <summary>
    /// Returns the paged dataset statistics 
    /// </summary>
    Task<IEnumerable<DataSetStatistics>> GetDataSetStatistics(int top, int skip, CancellationToken cancellationToken);
}