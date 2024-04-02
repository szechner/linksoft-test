using System.Collections.Immutable;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using SocialNetworkAnalyzer.App.DataSetStatistics.EventHandlers.DataSetCreated;
using SocialNetworkAnalyzer.App.Events;
using SocialNetworkAnalyzer.Core.Abstractions.Utils;
using SocialNetworkAnalyzer.Data.Abstraction;
using SocialNetworkAnalyzer.Data.EntityFramework.Contexts;
using SocialNetworkAnalyzer.Data.Repositories;
using SocialNetworkAnalyzer.Test.Integration.EntityFramework.Extensions;

namespace SocialNetworkAnalyzer.Test.Integration.App.DataSetStatistics;

[TestFixture]
public class DataSetCreatedTests
{
    private ServiceProvider serviceProvider = null!;

    [OneTimeSetUp]
    public async Task Setup()
    {
        var services = RepositoryTestsExtensions.CreateServiceCollection(nameof(DataSetCreatedTests).ToLower());

        serviceProvider = services.BuildServiceProvider();
        StaticLogger.Initialize(serviceProvider);

        using var scope = serviceProvider.CreateScope();
        await RepositoryTestsExtensions.CleanupTable(typeof(Data.Model.Database.DataSet), serviceProvider, CancellationToken.None);
        await RepositoryTestsExtensions.CleanupTable(typeof(Data.Model.Database.DataSetStatistics), serviceProvider, CancellationToken.None);
        await RepositoryTestsExtensions.CleanupTable(typeof(Data.Model.Database.Relationship), serviceProvider, CancellationToken.None);
        await RepositoryTestsExtensions.CleanupTable(typeof(Data.Model.Database.User), serviceProvider, CancellationToken.None);
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        var context = serviceProvider.GetRequiredService<SocialMappingContext>();
        context.Database.EnsureDeleted();
        serviceProvider.Dispose();
    }
    
    private async Task<Data.Model.Database.DataSet> CreateDataSet(string name, IServiceScope serviceScope)
    {
        var dataSetsRepository = serviceScope.ServiceProvider.GetRequiredService<IDataSetsRepository>();

        var dataSet = await dataSetsRepository.CreateDataSet(name, default);

        var transactionManager = serviceScope.ServiceProvider.GetRequiredService<ITransactionManager>();
        transactionManager.Commit();

        return dataSet;
    }
    
    [Test]
    [CancelAfter(90_000)]
    public async Task UpdateAvgRelationsCountEventHandler_Update_Statistics(CancellationToken cancellationToken)
    {
        var scope = serviceProvider.CreateScope();
        var dataSetStatisticsRepository = scope.ServiceProvider.GetRequiredService<IDataSetStatisticsRepository>();

        var dataSet = await CreateDataSet(nameof(UpdateAvgRelationsCountEventHandler_Update_Statistics), scope);

        _ = await dataSetStatisticsRepository.CreateDataSetStatistics(dataSet.Id, cancellationToken);

        var transactionManager = scope.ServiceProvider.GetRequiredService<ITransactionManager>();
        transactionManager.Commit();
        scope.Dispose();

        scope = serviceProvider.CreateScope();
        
        var handler = new UpdateAvgRelationsCountEventHandler(scope.ServiceProvider.GetRequiredService<IServiceScopeFactory>());

        await handler.Handle(new DataSetCreatedEvent(dataSet.Id), cancellationToken);
        
        scope.Dispose();
        scope = serviceProvider.CreateScope();
        
        dataSetStatisticsRepository = scope.ServiceProvider.GetRequiredService<IDataSetStatisticsRepository>();
        
        var dataSetStatistics = await dataSetStatisticsRepository.GetDataSetStatistics(100, 0, cancellationToken);
        
        dataSetStatistics.First(t => t.DataSetId == dataSet.Id).AvgRelationsCount.Should().Be(0);

        var relationshipsRepository = scope.ServiceProvider.GetRequiredService<IRelationshipsRepository>();
        var usersRepository = scope.ServiceProvider.GetRequiredService<IUsersRepository>();
        
        transactionManager = scope.ServiceProvider.GetRequiredService<ITransactionManager>();
        
        var buffer = new List<(int UserId1, int UserId2)>
        {
            (1, 2),
            (2, 3),
            (3, 4),
            (4, 1)
        };
        
        var users = buffer.SelectMany(x => new[] {x.UserId1, x.UserId2}).Distinct().ToImmutableArray();
        
        await usersRepository.Add(users, cancellationToken);
        
        await relationshipsRepository.Add(buffer.ToImmutableArray(), dataSet.Id, cancellationToken);       
        transactionManager.Commit();
        
        scope.Dispose();
        scope = serviceProvider.CreateScope();
        
        handler = new UpdateAvgRelationsCountEventHandler(scope.ServiceProvider.GetRequiredService<IServiceScopeFactory>());

        await handler.Handle(new DataSetCreatedEvent(dataSet.Id), cancellationToken);
        
        scope.Dispose();
        scope = serviceProvider.CreateScope();
        
        dataSetStatisticsRepository = scope.ServiceProvider.GetRequiredService<IDataSetStatisticsRepository>();
        
        dataSetStatistics = await dataSetStatisticsRepository.GetDataSetStatistics(100, 0, cancellationToken);
        
        dataSetStatistics.First(t => t.DataSetId == dataSet.Id).AvgRelationsCount.Should().Be(2);
        
        scope.Dispose();
        scope = serviceProvider.CreateScope();
        
        handler = new UpdateAvgRelationsCountEventHandler(scope.ServiceProvider.GetRequiredService<IServiceScopeFactory>());

        Func<Task> exception = () => handler.Handle(new DataSetCreatedEvent(0), cancellationToken);
        
        await exception.Should().ThrowAsync<ArgumentNullException>().WithMessage("DataSet with id 0 not found (Parameter 'dataSetId')");
    }
    
    [Test]
    [CancelAfter(90_000)]
    public async Task UpdateAvgGroupRelationsCountEventHandler_Update_Statistics(CancellationToken cancellationToken)
    {
        var scope = serviceProvider.CreateScope();
        var dataSetStatisticsRepository = scope.ServiceProvider.GetRequiredService<IDataSetStatisticsRepository>();

        var dataSet = await CreateDataSet(nameof(UpdateAvgGroupRelationsCountEventHandler_Update_Statistics), scope);

        _ = await dataSetStatisticsRepository.CreateDataSetStatistics(dataSet.Id, cancellationToken);

        var transactionManager = scope.ServiceProvider.GetRequiredService<ITransactionManager>();
        transactionManager.Commit();
        scope.Dispose();

        scope = serviceProvider.CreateScope();
        
        var handler = new UpdateAvgGroupRelationsCountEventHandler(scope.ServiceProvider.GetRequiredService<IServiceScopeFactory>());

        await handler.Handle(new DataSetCreatedEvent(dataSet.Id), cancellationToken);
        
        scope.Dispose();
        scope = serviceProvider.CreateScope();
        
        dataSetStatisticsRepository = scope.ServiceProvider.GetRequiredService<IDataSetStatisticsRepository>();
        
        var dataSetStatistics = await dataSetStatisticsRepository.GetDataSetStatistics(100, 0, cancellationToken);
        
        dataSetStatistics.First(t => t.DataSetId == dataSet.Id).AvgGroupRelationsCount.Should().Be(0);

        var relationshipsRepository = scope.ServiceProvider.GetRequiredService<IRelationshipsRepository>();
        var usersRepository = scope.ServiceProvider.GetRequiredService<IUsersRepository>();
        
        transactionManager = scope.ServiceProvider.GetRequiredService<ITransactionManager>();
        
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
        
        var users = buffer.SelectMany(x => new[] {x.UserId1, x.UserId2}).Distinct().ToImmutableArray();
        
        await usersRepository.Add(users, cancellationToken);
        
        await relationshipsRepository.Add(buffer.ToImmutableArray(), dataSet.Id, cancellationToken);       
        transactionManager.Commit();
        
        scope.Dispose();
        scope = serviceProvider.CreateScope();
        
        handler = new UpdateAvgGroupRelationsCountEventHandler(scope.ServiceProvider.GetRequiredService<IServiceScopeFactory>());

        await handler.Handle(new DataSetCreatedEvent(dataSet.Id), cancellationToken);
        
        scope.Dispose();
        scope = serviceProvider.CreateScope();
        
        dataSetStatisticsRepository = scope.ServiceProvider.GetRequiredService<IDataSetStatisticsRepository>();
        
        dataSetStatistics = await dataSetStatisticsRepository.GetDataSetStatistics(100, 0, cancellationToken);
        
        dataSetStatistics.First(t => t.DataSetId == dataSet.Id).AvgGroupRelationsCount.Should().Be(3.5);
        
        scope.Dispose();
        scope = serviceProvider.CreateScope();
        
        handler = new UpdateAvgGroupRelationsCountEventHandler(scope.ServiceProvider.GetRequiredService<IServiceScopeFactory>());

        Func<Task> exception = () => handler.Handle(new DataSetCreatedEvent(0), cancellationToken);
        
        await exception.Should().ThrowAsync<ArgumentNullException>().WithMessage("DataSet with id 0 not found (Parameter 'dataSetId')");
    }
    
    [Test]
    [CancelAfter(90_000)]
    public async Task UpdateNodesCountEventHandler_Update_Statistics(CancellationToken cancellationToken)
    {
        var scope = serviceProvider.CreateScope();
        var dataSetStatisticsRepository = scope.ServiceProvider.GetRequiredService<IDataSetStatisticsRepository>();

        var dataSet = await CreateDataSet(nameof(UpdateNodesCountEventHandler_Update_Statistics), scope);

        _ = await dataSetStatisticsRepository.CreateDataSetStatistics(dataSet.Id, cancellationToken);

        var transactionManager = scope.ServiceProvider.GetRequiredService<ITransactionManager>();
        transactionManager.Commit();
        scope.Dispose();

        scope = serviceProvider.CreateScope();
        
        var handler = new UpdateNodesCountEventHandler(scope.ServiceProvider.GetRequiredService<IServiceScopeFactory>());

        await handler.Handle(new DataSetCreatedEvent(dataSet.Id), cancellationToken);
        
        scope.Dispose();
        scope = serviceProvider.CreateScope();
        
        dataSetStatisticsRepository = scope.ServiceProvider.GetRequiredService<IDataSetStatisticsRepository>();
        
        var dataSetStatistics = await dataSetStatisticsRepository.GetDataSetStatistics(100, 0, cancellationToken);
        
        dataSetStatistics.First(t => t.DataSetId == dataSet.Id).NodesCount.Should().Be(0);

        var relationshipsRepository = scope.ServiceProvider.GetRequiredService<IRelationshipsRepository>();
        var usersRepository = scope.ServiceProvider.GetRequiredService<IUsersRepository>();
        
        transactionManager = scope.ServiceProvider.GetRequiredService<ITransactionManager>();
        
        var buffer = new List<(int UserId1, int UserId2)>
        {
            (1, 2),
            (2, 3),
            (3, 4),
            (4, 1)
        };
        
        var users = buffer.SelectMany(x => new[] {x.UserId1, x.UserId2}).Distinct().ToImmutableArray();
        
        await usersRepository.Add(users, cancellationToken);
        
        await relationshipsRepository.Add(buffer.ToImmutableArray(), dataSet.Id, cancellationToken);       
        transactionManager.Commit();
        
        scope.Dispose();
        scope = serviceProvider.CreateScope();
        
        handler = new UpdateNodesCountEventHandler(scope.ServiceProvider.GetRequiredService<IServiceScopeFactory>());

        await handler.Handle(new DataSetCreatedEvent(dataSet.Id), cancellationToken);
        
        scope.Dispose();
        scope = serviceProvider.CreateScope();
        
        dataSetStatisticsRepository = scope.ServiceProvider.GetRequiredService<IDataSetStatisticsRepository>();
        
        dataSetStatistics = await dataSetStatisticsRepository.GetDataSetStatistics(100, 0, cancellationToken);
        
        dataSetStatistics.First(t => t.DataSetId == dataSet.Id).NodesCount.Should().Be(4);
        
        scope.Dispose();
        scope = serviceProvider.CreateScope();
        
        handler = new UpdateNodesCountEventHandler(scope.ServiceProvider.GetRequiredService<IServiceScopeFactory>());

        Func<Task> exception = () => handler.Handle(new DataSetCreatedEvent(0), cancellationToken);
        
        await exception.Should().ThrowAsync<ArgumentNullException>().WithMessage("DataSet with id 0 not found (Parameter 'dataSetId')");
    }
    
    [Test]
    [CancelAfter(90_000)]
    public async Task UpdateRelationshipsCountEventHandler_Update_Statistics(CancellationToken cancellationToken)
    {
        var scope = serviceProvider.CreateScope();
        var dataSetStatisticsRepository = scope.ServiceProvider.GetRequiredService<IDataSetStatisticsRepository>();

        var dataSet = await CreateDataSet(nameof(UpdateRelationshipsCountEventHandler_Update_Statistics), scope);

        _ = await dataSetStatisticsRepository.CreateDataSetStatistics(dataSet.Id, cancellationToken);

        var transactionManager = scope.ServiceProvider.GetRequiredService<ITransactionManager>();
        transactionManager.Commit();
        scope.Dispose();

        scope = serviceProvider.CreateScope();
        
        var handler = new UpdateRelationshipsCountEventHandler(scope.ServiceProvider.GetRequiredService<IServiceScopeFactory>());

        await handler.Handle(new DataSetCreatedEvent(dataSet.Id), cancellationToken);
        
        scope.Dispose();
        scope = serviceProvider.CreateScope();
        
        dataSetStatisticsRepository = scope.ServiceProvider.GetRequiredService<IDataSetStatisticsRepository>();
        
        var dataSetStatistics = await dataSetStatisticsRepository.GetDataSetStatistics(100, 0, cancellationToken);
        
        dataSetStatistics.First(t => t.DataSetId == dataSet.Id).RelationshipsCount.Should().Be(0);

        var relationshipsRepository = scope.ServiceProvider.GetRequiredService<IRelationshipsRepository>();
        var usersRepository = scope.ServiceProvider.GetRequiredService<IUsersRepository>();
        
        transactionManager = scope.ServiceProvider.GetRequiredService<ITransactionManager>();
        
        var buffer = new List<(int UserId1, int UserId2)>
        {
            (1, 2),
            (2, 3),
            (3, 4),
            (4, 1)
        };
        
        var users = buffer.SelectMany(x => new[] {x.UserId1, x.UserId2}).Distinct().ToImmutableArray();
        
        await usersRepository.Add(users, cancellationToken);
        
        await relationshipsRepository.Add(buffer.ToImmutableArray(), dataSet.Id, cancellationToken);       
        transactionManager.Commit();
        
        scope.Dispose();
        scope = serviceProvider.CreateScope();
        
        handler = new UpdateRelationshipsCountEventHandler(scope.ServiceProvider.GetRequiredService<IServiceScopeFactory>());

        await handler.Handle(new DataSetCreatedEvent(dataSet.Id), cancellationToken);
        
        scope.Dispose();
        scope = serviceProvider.CreateScope();
        
        dataSetStatisticsRepository = scope.ServiceProvider.GetRequiredService<IDataSetStatisticsRepository>();
        
        dataSetStatistics = await dataSetStatisticsRepository.GetDataSetStatistics(100, 0, cancellationToken);
        
        dataSetStatistics.First(t => t.DataSetId == dataSet.Id).RelationshipsCount.Should().Be(8);
        
        scope.Dispose();
        scope = serviceProvider.CreateScope();
        
        handler = new UpdateRelationshipsCountEventHandler(scope.ServiceProvider.GetRequiredService<IServiceScopeFactory>());

        Func<Task> exception = () => handler.Handle(new DataSetCreatedEvent(0), cancellationToken);
        
        await exception.Should().ThrowAsync<ArgumentNullException>().WithMessage("DataSet with id 0 not found (Parameter 'dataSetId')");
    }
}