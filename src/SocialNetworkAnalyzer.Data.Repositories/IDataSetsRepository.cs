using SocialNetworkAnalyzer.Data.Abstraction;
using SocialNetworkAnalyzer.Data.Model.Database;

namespace SocialNetworkAnalyzer.Data.Repositories;

/// <summary>
/// Repository for <see cref="DataSet"/>
/// </summary>
public interface IDataSetsRepository : IDataRepository<DataSet>
{
    /// <summary>
    /// Create a new dataset 
    /// </summary>
    Task<DataSet> CreateDataSet(string name, CancellationToken cancellationToken);
}