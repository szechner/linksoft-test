using Microsoft.EntityFrameworkCore;

namespace SocialNetworkAnalyzer.Data.EntityFramework;

/// <summary>
/// Provider to handle the database schema
/// </summary>
public interface IDbSchemaProvider
{
    /// <summary>
    /// Ensure the database defined in <see cref="DbContext"/> is created
    /// </summary>
    void EnsureDbCreated(DbContext context);
}

public class DbSchemaProvider : IDbSchemaProvider
{
    private bool isDbCreated;

    private object creatingLock = new();

    /// <inheritdoc />
    public void EnsureDbCreated(DbContext context)
    {
        if (isDbCreated) return;
        lock (creatingLock)
        {
            isDbCreated = !context.Database.EnsureCreated();
            if (isDbCreated) return;


            if (!isDbCreated)
            {
                isDbCreated = !context.Database.EnsureCreated();
            }
        }
    }
}