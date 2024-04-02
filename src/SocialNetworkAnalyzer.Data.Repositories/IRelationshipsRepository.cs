using System.Collections.Immutable;
using SocialNetworkAnalyzer.Data.Abstraction;
using SocialNetworkAnalyzer.Data.Model.Database;

namespace SocialNetworkAnalyzer.Data.Repositories;

/// <summary>
/// Repository for <see cref="Relationship"/>
/// </summary>
public interface IRelationshipsRepository: IDataRepository<Relationship>
{
    /// <summary>
    /// Add relationships to the database 
    /// </summary>
    Task Add(ImmutableArray<(int UserId1, int UserId2)> buffer, int dataSetId, CancellationToken cancellationToken);
    
    /// <summary>
    /// Returns the number of relationships in the dataset 
    /// </summary>
    Task<int> GetRelationshipsCount(int dataSetId, CancellationToken cancellationToken);
    
    /// <summary>
    /// Returns the number of unique users in the dataset 
    /// </summary>
    Task<int> GetUniqueUsersCount(int dataSetId, CancellationToken cancellationToken);
    
    /// <summary>
    /// Returns the average number of relationships per user in the dataset 
    /// </summary>
    Task<double> GetAvgRelationsCount(int dataSetId, CancellationToken cancellationToken);

    /// <summary>
    ///  Returns the average number of relationships per user group in the dataset 
    /// </summary>
    Task<double> GetAvgGroupRelationsCount(int dataSetId, CancellationToken cancellationToken);
}