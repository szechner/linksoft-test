using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SocialNetworkAnalyzer.Data.EntityFramework.Contexts;
using SocialNetworkAnalyzer.Data.EntityFramework.Extensions;

namespace SocialNetworkAnalyzer.Test.Integration.EntityFramework.Extensions;

public static class RepositoryTestsExtensions
{
    public static IServiceCollection CreateServiceCollection(string databaseName)
    {
        var services = new ServiceCollection();

        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Debug);
        });
        services.AddEntityFramework($"Host=localhost;Port=5432;Username=postgres;Password=password;Database={databaseName};");

        return services;
    }

    public static async Task CleanupTable(Type entityType, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SocialMappingContext>();
#pragma warning disable EF1002
        await context.Database.ExecuteSqlRawAsync($"TRUNCATE TABLE \"{context.Model.FindEntityType(entityType)?.GetSchemaQualifiedTableName()}\" RESTART IDENTITY CASCADE;", cancellationToken);
#pragma warning restore EF1002
        context.Commit();
    }
}