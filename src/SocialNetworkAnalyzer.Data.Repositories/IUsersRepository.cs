using System.Collections.Immutable;
using SocialNetworkAnalyzer.Data.Abstraction;
using SocialNetworkAnalyzer.Data.Model.Database;

namespace SocialNetworkAnalyzer.Data.Repositories;

/// <summary>
/// Repository for <see cref="User"/>
/// </summary>
public interface IUsersRepository : IDataRepository<User>
{
    /// <summary>
    /// Add users to the database 
    /// </summary>
    Task Add(ImmutableArray<int> buffer, CancellationToken cancellationToken);
}