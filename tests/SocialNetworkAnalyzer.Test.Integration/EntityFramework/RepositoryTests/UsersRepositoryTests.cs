using System.Collections.Immutable;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SocialNetworkAnalyzer.Core.Abstractions.Utils;
using SocialNetworkAnalyzer.Data.Abstraction;
using SocialNetworkAnalyzer.Data.EntityFramework.Contexts;
using SocialNetworkAnalyzer.Data.Model.Database;
using SocialNetworkAnalyzer.Data.Repositories;
using SocialNetworkAnalyzer.Test.Integration.EntityFramework.Extensions;

namespace SocialNetworkAnalyzer.Test.Integration.EntityFramework.RepositoryTests;

[TestFixture]
[Parallelizable(ParallelScope.All)]
public class UsersRepositoryTests
{
    private ServiceProvider serviceProvider = null!;

    [OneTimeSetUp]
    public async Task Setup()
    {
        var services = RepositoryTestsExtensions.CreateServiceCollection(nameof(UsersRepositoryTests).ToLower());

        serviceProvider = services.BuildServiceProvider();
        StaticLogger.Initialize(serviceProvider);

        using var scope = serviceProvider.CreateScope();
        await RepositoryTestsExtensions.CleanupTable(typeof(User), serviceProvider, CancellationToken.None);
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        var context = serviceProvider.GetRequiredService<SocialMappingContext>();
        context.Database.EnsureDeleted();
        serviceProvider.Dispose();
    }

    [Test]
    [CancelAfter(90_000)]
    public async Task Add_Users_To_Database_With_Add(CancellationToken cancellationToken)
    {
        var scope = serviceProvider.CreateScope();
        var usersRepository = scope.ServiceProvider.GetRequiredService<IUsersRepository>();

        var buffer = new HashSet<int>();

        for (var i = 0; i < 10_000; i++)
        {
            buffer.Add(Random.Shared.Next(100, 10000));
        }

        await usersRepository.Add(buffer.ToImmutableArray(), cancellationToken);
        
        var transactionManager = scope.ServiceProvider.GetRequiredService<ITransactionManager>();
        transactionManager.Commit();
        scope.Dispose();

        scope = serviceProvider.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<SocialMappingContext>();

        var usersDbSet = context.Set<User>();

        var users = await usersDbSet.CountAsync(cancellationToken);

        users.Should().Be(buffer.Distinct().Count());
        
        scope.Dispose();
    }
}