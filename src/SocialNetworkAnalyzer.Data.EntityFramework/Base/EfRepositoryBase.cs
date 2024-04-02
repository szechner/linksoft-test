using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SocialNetworkAnalyzer.Data.Abstraction;

namespace SocialNetworkAnalyzer.Data.EntityFramework.Base;

/// <summary>
/// Base class for EntityFramework repository 
/// </summary>
public abstract class EfRepositoryBase<T>(DbContext dbContext, ILogger logger) : IDataRepository<T>
    where T : class, IDbModel
{
    protected readonly DbContext DbContext = dbContext;
    protected readonly DbSet<T> DbSet = dbContext.Set<T>();
    protected readonly ILogger Logger = logger;
}