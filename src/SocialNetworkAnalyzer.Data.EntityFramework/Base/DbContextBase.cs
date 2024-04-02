using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using SocialNetworkAnalyzer.Core.Guards;
using SocialNetworkAnalyzer.Data.Abstraction;

namespace SocialNetworkAnalyzer.Data.EntityFramework.Base;

/// <summary>
/// Base class for <see cref="DbContext"/> with transaction management 
/// </summary>
public class DbContextBase<T> : DbContext, ITransactionManager
    where T : DbContext
{
    private IDbContextTransaction? transaction;
    protected readonly ILogger<T> Logger;

    public DbContextBase(DbContextOptions<T> options, ILogger<T> logger, IDbSchemaProvider dbSchemaProvider)
        : base(options)
    {
        _ = Guard.Require.ArgumentNotNull(options);
        Logger = logger;
        dbSchemaProvider.EnsureDbCreated(this);
        ResetTransaction();
    }

    /// <inheritdoc cref="ITransactionManager.Commit"/>
    public void Commit()
    {
        transaction?.Commit();
        Logger.LogDebug("Transaction committed from {DbContext}", GetType().Name);
        ResetTransaction();
    }

    /// <inheritdoc cref="ITransactionManager.Rollback"/>
    public void Rollback()
    {
        transaction?.Rollback();
        Logger.LogDebug("Transaction rolled back from {DbContext}", GetType().Name);
        ResetTransaction();
    }

    /// <summary>
    /// Reuse existing or create a new <see cref="IDbContextTransaction"/>
    /// </summary>
    private void ResetTransaction()
    {
        transaction = Database.CurrentTransaction ?? Database.BeginTransaction();
    }

    /// <summary>
    /// Implementation of the dispose pattern
    /// </summary>
    public override void Dispose()
    {
        transaction?.Dispose();
        base.Dispose();
        GC.SuppressFinalize(this);
    }
}