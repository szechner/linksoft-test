using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SocialNetworkAnalyzer.Data.Abstraction;
using SocialNetworkAnalyzer.Data.EntityFramework.Contexts;

namespace SocialNetworkAnalyzer.Data.EntityFramework.Extensions;

/// <summary>
/// <see cref="IServiceCollection" /> extension methods for configuring Entity Framework services. 
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Method to add Entity Framework services to the service collection.
    /// </summary>
    public static void AddEntityFramework(this IServiceCollection services, string connectionString)
    {
        // Use scoped DbContext factory
        services.AddDbContext<SocialMappingContext>(
            o => o
                .UseNpgsql(connectionString)
                .UseSnakeCaseNamingConvention() // Use snake case naming convention - postgres default
        );

        // We need to ensure the database is created and store this state in memory
        services.AddSingleton<IDbSchemaProvider, DbSchemaProvider>();
        services.AddScoped<ITransactionManager>(sp => sp.GetRequiredService<SocialMappingContext>());

        // Register repositories
        services.AddRepositories();
    }

    /// <summary>
    /// Add all repositories to the service collection.
    /// </summary>
    /// <param name="services"></param>
    private static void AddRepositories(this IServiceCollection services)
    {
        var repositories = DataEntityFrameworkAssembly.Assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.GetInterfaces().Any(t => t.Name.StartsWith("IDataRepository")))
            .ToList();

        foreach (var repository in repositories)
        {
            var interfaceType = repository.GetInterfaces().First(i => !i.IsGenericType);
            services.AddScoped(interfaceType, repository);
        }
    }
}