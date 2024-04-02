namespace SocialNetworkAnalyzer.Data.Abstraction;

/// <summary>
/// Interface for transaction manager
/// </summary>
public interface ITransactionManager
{
    /// <summary>
    /// Perform a commit action
    /// </summary>
    void Commit();
    
    /// <summary>
    /// Perform a rollback action
    /// </summary>
    void Rollback();
}