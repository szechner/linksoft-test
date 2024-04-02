namespace SocialNetworkAnalyzer.Data.Abstraction;

/// <summary>
/// Interface for repository which provides database operations over a <see cref="IDbModel"/> 
/// </summary>
public interface IDataRepository<T> where T : IDbModel;