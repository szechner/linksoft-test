using System.Collections.Immutable;
using FluentAssertions;
using FluentValidation;
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
public class RelationshipsRepositoryTests
{
    private ServiceProvider serviceProvider = null!;

    [OneTimeSetUp]
    public async Task Setup()
    {
        var services = RepositoryTestsExtensions.CreateServiceCollection(nameof(RelationshipsRepositoryTests).ToLower());

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
    public async Task Add_RelationShips_To_Database(CancellationToken cancellationToken)
    {
        var scope = serviceProvider.CreateScope();
        var transactionManager = scope.ServiceProvider.GetRequiredService<ITransactionManager>();
        var dataSetsRepository = scope.ServiceProvider.GetRequiredService<IDataSetsRepository>();
        var usersRepository = scope.ServiceProvider.GetRequiredService<IUsersRepository>();
        var relationshipsRepository = scope.ServiceProvider.GetRequiredService<IRelationshipsRepository>();

        var dataSet = await dataSetsRepository.CreateDataSet("TestDataSet", cancellationToken);

        var buffer = new List<(int UserId1, int UserId2)>();

        for (var i = 0; i < 10_000; i++)
        {
            var userId1 = Random.Shared.Next(100, 10000);
            var userId2 = Random.Shared.Next(100, 10000);

            if (userId1 == userId2)
            {
                userId2++;
            }

            buffer.Add((userId1, userId2));
        }

        var users = buffer.SelectMany(x => new[] { x.UserId1, x.UserId2 }).Distinct().ToImmutableArray();

        await usersRepository.Add(users, cancellationToken);

        await relationshipsRepository.Add(buffer.ToImmutableArray(), dataSet.Id, cancellationToken);

        transactionManager.Commit();

        scope.Dispose();

        scope = serviceProvider.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<SocialMappingContext>();

        var relationShipsDbSet = context.Set<Relationship>();

        var relationships = await relationShipsDbSet.Where(t => t.DataSetId == dataSet.Id).CountAsync(cancellationToken);

        relationships.Should().Be(buffer.Distinct().Count());

        scope.Dispose();
    }

    [Test]
    [CancelAfter(90_000)]
    public async Task Throw_Self_Referenciong_Relations(CancellationToken cancellationToken)
    {
        var scope = serviceProvider.CreateScope();
        var dataSetsRepository = scope.ServiceProvider.GetRequiredService<IDataSetsRepository>();
        var usersRepository = scope.ServiceProvider.GetRequiredService<IUsersRepository>();
        var relationshipsRepository = scope.ServiceProvider.GetRequiredService<IRelationshipsRepository>();

        var dataSet = await dataSetsRepository.CreateDataSet("TestDataSet", cancellationToken);

        var buffer = new List<(int UserId1, int UserId2)>();

        for (var i = 0; i < 10; i++)
        {
            var userId1 = Random.Shared.Next(100, 10000);
            var userId2 = Random.Shared.Next(100, 10000);

            if (userId1 == userId2)
            {
                userId2++;
            }

            buffer.Add((userId1, userId2));
        }

        buffer.Add((500, 500));

        var users = buffer.SelectMany(x => new[] { x.UserId1, x.UserId2 }).Distinct().ToImmutableArray();

        await usersRepository.Add(users, cancellationToken);

        Func<Task> selfReferencingException = async () => await relationshipsRepository.Add(buffer.ToImmutableArray(), dataSet.Id, cancellationToken);

        await selfReferencingException.Should().ThrowAsync<ValidationException>().Where(e =>
            e.Errors.Count() == 1 &&
            e.Errors.First().PropertyName == "Batch" &&
            e.Errors.First().ErrorMessage == $"User cannot have relation with itself:{Environment.NewLine}500 -> 500"
        );

        scope.Dispose();
    }

    [Test]
    [CancelAfter(90_000)]
    public async Task Count_RelationShips(CancellationToken cancellationToken)
    {
        var scope = serviceProvider.CreateScope();
        var transactionManager = scope.ServiceProvider.GetRequiredService<ITransactionManager>();
        var dataSetsRepository = scope.ServiceProvider.GetRequiredService<IDataSetsRepository>();
        var usersRepository = scope.ServiceProvider.GetRequiredService<IUsersRepository>();
        var relationshipsRepository = scope.ServiceProvider.GetRequiredService<IRelationshipsRepository>();

        var dataSet = await dataSetsRepository.CreateDataSet("TestDataSet1", cancellationToken);

        var buffer = new List<(int UserId1, int UserId2)>
        {
            (1, 2),
            (2, 3),
            (3, 4)
        };

        var users = buffer.SelectMany(x => new[] { x.UserId1, x.UserId2 }).Distinct().ToImmutableArray();

        await usersRepository.Add(users, cancellationToken);

        await relationshipsRepository.Add(buffer.ToImmutableArray(), dataSet.Id, cancellationToken);

        transactionManager.Commit();

        var relationShipsCount = await relationshipsRepository.GetRelationshipsCount(dataSet.Id, cancellationToken);

        relationShipsCount.Should().Be(buffer.Count * 2);

        scope.Dispose();
    }

    [Test]
    [CancelAfter(90_000)]
    public async Task Count_Unique_Users_RelationShips(CancellationToken cancellationToken)
    {
        var scope = serviceProvider.CreateScope();
        var transactionManager = scope.ServiceProvider.GetRequiredService<ITransactionManager>();
        var dataSetsRepository = scope.ServiceProvider.GetRequiredService<IDataSetsRepository>();
        var usersRepository = scope.ServiceProvider.GetRequiredService<IUsersRepository>();
        var relationshipsRepository = scope.ServiceProvider.GetRequiredService<IRelationshipsRepository>();

        var dataSet = await dataSetsRepository.CreateDataSet("TestDataSet2", cancellationToken);

        var buffer = new List<(int UserId1, int UserId2)>
        {
            (1, 2),
            (1, 3),
            (1, 4),
            (2, 1),
            (2, 3),
            (2, 4)
        };

        var users = buffer.SelectMany(x => new[] { x.UserId1, x.UserId2 }).Distinct().ToImmutableArray();

        await usersRepository.Add(users, cancellationToken);

        await relationshipsRepository.Add(buffer.ToImmutableArray(), dataSet.Id, cancellationToken);

        transactionManager.Commit();

        var relationShipsCount = await relationshipsRepository.GetUniqueUsersCount(dataSet.Id, cancellationToken);

        relationShipsCount.Should().Be(4);

        scope.Dispose();
    }

    [Test]
    [CancelAfter(90_000)]
    public async Task Avg_Count_RelationShips(CancellationToken cancellationToken)
    {
        var scope = serviceProvider.CreateScope();
        var transactionManager = scope.ServiceProvider.GetRequiredService<ITransactionManager>();
        var dataSetsRepository = scope.ServiceProvider.GetRequiredService<IDataSetsRepository>();
        var usersRepository = scope.ServiceProvider.GetRequiredService<IUsersRepository>();
        var relationshipsRepository = scope.ServiceProvider.GetRequiredService<IRelationshipsRepository>();

        var dataSet = await dataSetsRepository.CreateDataSet("TestDataSet3", cancellationToken);

        var buffer = new List<(int UserId1, int UserId2)>
        {
            (1, 2),
            (2, 3),
            (3, 4),
            (4, 1)
        };

        var users = buffer.SelectMany(x => new[] { x.UserId1, x.UserId2 }).Distinct().ToImmutableArray();

        await usersRepository.Add(users, cancellationToken);

        await relationshipsRepository.Add(buffer.ToImmutableArray(), dataSet.Id, cancellationToken);

        transactionManager.Commit();

        var relationShipsCount = await relationshipsRepository.GetAvgRelationsCount(dataSet.Id, cancellationToken);

        relationShipsCount.Should().Be(2);

        buffer.Add((1, 3));
        buffer.Add((2, 4));
        buffer.Add((3, 1));
        buffer.Add((4, 2));

        await relationshipsRepository.Add(buffer.ToImmutableArray(), dataSet.Id, cancellationToken);

        transactionManager.Commit();

        relationShipsCount = await relationshipsRepository.GetAvgRelationsCount(dataSet.Id, cancellationToken);

        relationShipsCount.Should().Be(3);

        // New relations with switch UserId1 and UserId2
        buffer.Add((2, 1));
        buffer.Add((3, 2));
        buffer.Add((4, 3));
        buffer.Add((1, 4));

        await relationshipsRepository.Add(buffer.ToImmutableArray(), dataSet.Id, cancellationToken);

        transactionManager.Commit();

        relationShipsCount = await relationshipsRepository.GetAvgRelationsCount(dataSet.Id, cancellationToken);

        relationShipsCount.Should().Be(3);

        scope.Dispose();
    }

    [Test]
    [CancelAfter(90_000)]
    public async Task Avg_Count_RelationShips_Groups(CancellationToken cancellationToken)
    {
        var scope = serviceProvider.CreateScope();
        var transactionManager = scope.ServiceProvider.GetRequiredService<ITransactionManager>();
        var dataSetsRepository = scope.ServiceProvider.GetRequiredService<IDataSetsRepository>();
        var usersRepository = scope.ServiceProvider.GetRequiredService<IUsersRepository>();
        var relationshipsRepository = scope.ServiceProvider.GetRequiredService<IRelationshipsRepository>();

        var dataSet = await dataSetsRepository.CreateDataSet("TestDataSet4", cancellationToken);

        var buffer = new List<(int UserId1, int UserId2)>
        {
            (1, 2),
            (1, 3),
            (1, 4),
            (2, 3),
            (2, 4),
            (3, 4),
            (5, 6),
            (4, 5),
            (4, 6)
        };

        var users = buffer.SelectMany(x => new[] { x.UserId1, x.UserId2 }).Distinct().ToImmutableArray();

        await usersRepository.Add(users, cancellationToken);

        await relationshipsRepository.Add(buffer.ToImmutableArray(), dataSet.Id, cancellationToken);

        transactionManager.Commit();

        var relationShipsCount = await relationshipsRepository.GetAvgGroupRelationsCount(dataSet.Id, cancellationToken);

        relationShipsCount.Should().Be(3.5);

        scope.Dispose();
    }
}