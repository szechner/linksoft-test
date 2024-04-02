using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using SocialNetworkAnalyzer.Core.Abstractions.Utils;
using SocialNetworkAnalyzer.Data.Abstraction;
using SocialNetworkAnalyzer.Data.EntityFramework.Contexts;
using SocialNetworkAnalyzer.Data.Model.Database;
using SocialNetworkAnalyzer.Data.Repositories;
using SocialNetworkAnalyzer.Test.Integration.EntityFramework.Extensions;

namespace SocialNetworkAnalyzer.Test.Integration.EntityFramework.RepositoryTests;

[TestFixture]
public class DatasetStatisticsRepositoryTests
{
    private ServiceProvider serviceProvider = null!;

    private ServiceProvider serviceProvider2 = null!;

    [OneTimeSetUp]
    public async Task Setup()
    {
        var services = RepositoryTestsExtensions.CreateServiceCollection(nameof(DatasetStatisticsRepositoryTests).ToLower());
        var services2 = RepositoryTestsExtensions.CreateServiceCollection($"{nameof(DatasetStatisticsRepositoryTests).ToLower()}2");

        serviceProvider = services.BuildServiceProvider();
        serviceProvider2 = services2.BuildServiceProvider();

        StaticLogger.Initialize(serviceProvider);
        StaticLogger.Initialize(serviceProvider2);

        await RepositoryTestsExtensions.CleanupTable(typeof(DataSetStatistics), serviceProvider, CancellationToken.None);

        await RepositoryTestsExtensions.CleanupTable(typeof(DataSetStatistics), serviceProvider2, CancellationToken.None);
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        var context = serviceProvider.GetRequiredService<SocialMappingContext>();
        context.Database.EnsureDeleted();
        serviceProvider.Dispose();

        var context2 = serviceProvider2.GetRequiredService<SocialMappingContext>();
        context2.Database.EnsureDeleted();
        serviceProvider2.Dispose();
    }

    private async Task<DataSet> CreateDataSet(string name, IServiceScope serviceScope)
    {
        var dataSetsRepository = serviceScope.ServiceProvider.GetRequiredService<IDataSetsRepository>();

        var dataSet = await dataSetsRepository.CreateDataSet(name, default);

        var transactionManager = serviceScope.ServiceProvider.GetRequiredService<ITransactionManager>();
        transactionManager.Commit();

        return dataSet;
    }

    [Test]
    [CancelAfter(90_000)]
    public async Task Get_Paged_And_Count_DataSetStatistics(CancellationToken cancellationToken)
    {
        var scope = serviceProvider2.CreateScope();
        var dataSetStatisticsRepository = scope.ServiceProvider.GetRequiredService<IDataSetStatisticsRepository>();

        for (int i = 0; i < 10; i++)
        {
            var dataSet = await CreateDataSet($"CreateDataSetStatistics{i}", scope);
            _ = await dataSetStatisticsRepository.CreateDataSetStatistics(dataSet.Id, cancellationToken);
        }


        var transactionManager = scope.ServiceProvider.GetRequiredService<ITransactionManager>();
        transactionManager.Commit();
        scope.Dispose();

        scope = serviceProvider2.CreateScope();


        dataSetStatisticsRepository = scope.ServiceProvider.GetRequiredService<IDataSetStatisticsRepository>();
        var dataSetStatisticsTop10 = await dataSetStatisticsRepository.GetDataSetStatistics(10, 0, cancellationToken);

        dataSetStatisticsTop10.Count().Should().Be(10);
        var dataSetStatisticsNext10 = await dataSetStatisticsRepository.GetDataSetStatistics(10, 10, cancellationToken);

        dataSetStatisticsNext10.Count().Should().Be(0);
        
        var dataSetStatisticsCount = await dataSetStatisticsRepository.CountRows(cancellationToken);

        dataSetStatisticsCount.Should().Be(10);
        
        scope.Dispose();
    }

    [Test]
    [CancelAfter(90_000)]
    public async Task Add_DataSetStatistics_To_Database_With_CreateDataSetStatistics(CancellationToken cancellationToken)
    {
        var scope = serviceProvider.CreateScope();
        var dataSetStatisticsRepository = scope.ServiceProvider.GetRequiredService<IDataSetStatisticsRepository>();

        var dataSet = await CreateDataSet("CreateDataSetStatistics", scope);

        _ = await dataSetStatisticsRepository.CreateDataSetStatistics(dataSet.Id, cancellationToken);

        var transactionManager = scope.ServiceProvider.GetRequiredService<ITransactionManager>();
        transactionManager.Commit();
        scope.Dispose();

        scope = serviceProvider.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<SocialMappingContext>();

        var dataSetStatisticsDbSet = context.Set<DataSetStatistics>();

        var dbDataSetStatistics = await dataSetStatisticsDbSet.FindAsync([dataSet.Id], cancellationToken);

        dbDataSetStatistics.Should().NotBeNull();
        dbDataSetStatistics!.State.Should().Be(DataSetStatisticsState.Pending);

        scope.Dispose();
    }


    [Test]
    [CancelAfter(90_000)]
    public async Task Update_DataSetStatistics_NodesCount(CancellationToken cancellationToken)
    {
        var scope = serviceProvider.CreateScope();
        var dataSetStatisticsRepository = scope.ServiceProvider.GetRequiredService<IDataSetStatisticsRepository>();

        var dataSet = await CreateDataSet("NodesCount", scope);

        _ = await dataSetStatisticsRepository.CreateDataSetStatistics(dataSet.Id, cancellationToken);

        var transactionManager = scope.ServiceProvider.GetRequiredService<ITransactionManager>();
        transactionManager.Commit();
        scope.Dispose();

        scope = serviceProvider.CreateScope();

        dataSetStatisticsRepository = scope.ServiceProvider.GetRequiredService<IDataSetStatisticsRepository>();

        await dataSetStatisticsRepository.UpdateNodesCount(dataSet.Id, 100, cancellationToken);

        transactionManager = scope.ServiceProvider.GetRequiredService<ITransactionManager>();
        transactionManager.Commit();
        scope.Dispose();

        scope = serviceProvider.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<SocialMappingContext>();

        var dataSetStatisticsDbSet = context.Set<DataSetStatistics>();

        var dbDataSetStatistics = await dataSetStatisticsDbSet.FindAsync([dataSet.Id], cancellationToken);

        dbDataSetStatistics.Should().NotBeNull();
        dbDataSetStatistics!.NodesCount.Should().Be(100);
        dbDataSetStatistics!.RelationshipsCount.Should().BeNull();
        dbDataSetStatistics!.AvgRelationsCount.Should().BeNull();
        dbDataSetStatistics!.State.Should().Be(DataSetStatisticsState.Pending);

        scope.Dispose();
    }

    [Test]
    [CancelAfter(90_000)]
    public async Task Update_DataSetStatistics_RelationshipsCount(CancellationToken cancellationToken)
    {
        var scope = serviceProvider.CreateScope();
        var dataSetStatisticsRepository = scope.ServiceProvider.GetRequiredService<IDataSetStatisticsRepository>();

        var dataSet = await CreateDataSet("RelationshipsCount", scope);

        _ = await dataSetStatisticsRepository.CreateDataSetStatistics(dataSet.Id, cancellationToken);

        var transactionManager = scope.ServiceProvider.GetRequiredService<ITransactionManager>();
        transactionManager.Commit();
        scope.Dispose();

        scope = serviceProvider.CreateScope();

        dataSetStatisticsRepository = scope.ServiceProvider.GetRequiredService<IDataSetStatisticsRepository>();

        await dataSetStatisticsRepository.UpdateRelationshipsCount(dataSet.Id, 100, cancellationToken);

        transactionManager = scope.ServiceProvider.GetRequiredService<ITransactionManager>();
        transactionManager.Commit();
        scope.Dispose();

        scope = serviceProvider.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<SocialMappingContext>();

        var dataSetStatisticsDbSet = context.Set<DataSetStatistics>();

        var dbDataSetStatistics = await dataSetStatisticsDbSet.FindAsync([dataSet.Id], cancellationToken);

        dbDataSetStatistics.Should().NotBeNull();
        dbDataSetStatistics!.RelationshipsCount.Should().Be(100);
        dbDataSetStatistics!.NodesCount.Should().BeNull();
        dbDataSetStatistics!.AvgRelationsCount.Should().BeNull();
        dbDataSetStatistics!.State.Should().Be(DataSetStatisticsState.Pending);

        scope.Dispose();
    }

    [Test]
    [CancelAfter(90_000)]
    public async Task Update_DataSetStatistics_AvgRelationsCount(CancellationToken cancellationToken)
    {
        var scope = serviceProvider.CreateScope();
        var dataSetStatisticsRepository = scope.ServiceProvider.GetRequiredService<IDataSetStatisticsRepository>();

        var dataSet = await CreateDataSet("AvgRelationsCount", scope);

        _ = await dataSetStatisticsRepository.CreateDataSetStatistics(dataSet.Id, cancellationToken);

        var transactionManager = scope.ServiceProvider.GetRequiredService<ITransactionManager>();
        transactionManager.Commit();
        scope.Dispose();

        scope = serviceProvider.CreateScope();

        dataSetStatisticsRepository = scope.ServiceProvider.GetRequiredService<IDataSetStatisticsRepository>();

        await dataSetStatisticsRepository.UpdateAvgRelationsCount(dataSet.Id, 100.55, cancellationToken);

        transactionManager = scope.ServiceProvider.GetRequiredService<ITransactionManager>();
        transactionManager.Commit();
        scope.Dispose();

        scope = serviceProvider.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<SocialMappingContext>();

        var dataSetStatisticsDbSet = context.Set<DataSetStatistics>();

        var dbDataSetStatistics = await dataSetStatisticsDbSet.FindAsync([dataSet.Id], cancellationToken);

        dbDataSetStatistics.Should().NotBeNull();
        dbDataSetStatistics!.RelationshipsCount.Should().BeNull();
        dbDataSetStatistics!.NodesCount.Should().BeNull();
        dbDataSetStatistics!.AvgRelationsCount.Should().Be(100.55);
        dbDataSetStatistics!.State.Should().Be(DataSetStatisticsState.Pending);

        scope.Dispose();
    }

    [Test]
    [CancelAfter(90_000)]
    public async Task Update_DataSetStatistics_StateCalculated(CancellationToken cancellationToken)
    {
        var scope = serviceProvider.CreateScope();
        var dataSetStatisticsRepository = scope.ServiceProvider.GetRequiredService<IDataSetStatisticsRepository>();

        var dataSet = await CreateDataSet("StateCalculated", scope);

        _ = await dataSetStatisticsRepository.CreateDataSetStatistics(dataSet.Id, cancellationToken);

        var transactionManager = scope.ServiceProvider.GetRequiredService<ITransactionManager>();
        transactionManager.Commit();
        scope.Dispose();

        scope = serviceProvider.CreateScope();

        dataSetStatisticsRepository = scope.ServiceProvider.GetRequiredService<IDataSetStatisticsRepository>();

        await dataSetStatisticsRepository.UpdateNodesCount(dataSet.Id, 100, cancellationToken);
        await dataSetStatisticsRepository.UpdateRelationshipsCount(dataSet.Id, 100, cancellationToken);
        await dataSetStatisticsRepository.UpdateAvgRelationsCount(dataSet.Id, 100.55, cancellationToken);
        await dataSetStatisticsRepository.UpdateAvgGroupRelationsCount(dataSet.Id, 10.55, cancellationToken);

        transactionManager = scope.ServiceProvider.GetRequiredService<ITransactionManager>();
        transactionManager.Commit();
        scope.Dispose();

        scope = serviceProvider.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<SocialMappingContext>();

        var dataSetStatisticsDbSet = context.Set<DataSetStatistics>();

        var dbDataSetStatistics = await dataSetStatisticsDbSet.FindAsync([dataSet.Id], cancellationToken);

        dbDataSetStatistics.Should().NotBeNull();
        dbDataSetStatistics!.RelationshipsCount.Should().Be(100);
        dbDataSetStatistics!.NodesCount.Should().Be(100);
        dbDataSetStatistics!.AvgRelationsCount.Should().Be(100.55);
        dbDataSetStatistics!.AvgGroupRelationsCount.Should().Be(10.55);
        dbDataSetStatistics!.State.Should().Be(DataSetStatisticsState.Calculated);

        scope.Dispose();
    }

    [Test]
    [CancelAfter(90_000)]
    public async Task Update_DataSetStatistics_StateError(CancellationToken cancellationToken)
    {
        var scope = serviceProvider.CreateScope();
        var dataSetStatisticsRepository = scope.ServiceProvider.GetRequiredService<IDataSetStatisticsRepository>();

        var dataSet = await CreateDataSet("StateCalculated", scope);

        _ = await dataSetStatisticsRepository.CreateDataSetStatistics(dataSet.Id, cancellationToken);

        var transactionManager = scope.ServiceProvider.GetRequiredService<ITransactionManager>();
        transactionManager.Commit();
        scope.Dispose();

        scope = serviceProvider.CreateScope();

        dataSetStatisticsRepository = scope.ServiceProvider.GetRequiredService<IDataSetStatisticsRepository>();

        await dataSetStatisticsRepository.SetError(dataSet.Id, nameof(Update_DataSetStatistics_StateError), cancellationToken);

        transactionManager = scope.ServiceProvider.GetRequiredService<ITransactionManager>();
        transactionManager.Commit();
        scope.Dispose();

        scope = serviceProvider.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<SocialMappingContext>();

        var dataSetStatisticsDbSet = context.Set<DataSetStatistics>();

        var dbDataSetStatistics = await dataSetStatisticsDbSet.FindAsync([dataSet.Id], cancellationToken);

        dbDataSetStatistics.Should().NotBeNull();
        dbDataSetStatistics!.Error.Should().Be(nameof(Update_DataSetStatistics_StateError));
        dbDataSetStatistics!.State.Should().Be(DataSetStatisticsState.Error);

        scope.Dispose();
    }

    [Test]
    [CancelAfter(90_000)]
    public async Task DataSet_NotFound_Error(CancellationToken cancellationToken)
    {
        var scope = serviceProvider.CreateScope();
        var dataSetStatisticsRepository = scope.ServiceProvider.GetRequiredService<IDataSetStatisticsRepository>();

        Func<Task> errorAction = async () => await dataSetStatisticsRepository.SetError(100_000, nameof(DataSet_NotFound_Error), cancellationToken);

        await errorAction.Should().ThrowAsync<ArgumentNullException>().WithMessage("DataSet with id 100000 not found (Parameter 'dataSetId')");

        scope.Dispose();
    }

    [Test]
    [CancelAfter(90_000)]
    public async Task DataSetStatistics_NotFound_Error(CancellationToken cancellationToken)
    {
        var scope = serviceProvider.CreateScope();
        var dataSet = await CreateDataSet("StateCalculated", scope);

        var transactionManager = scope.ServiceProvider.GetRequiredService<ITransactionManager>();
        transactionManager.Commit();
        scope.Dispose();

        scope = serviceProvider.CreateScope();

        var dataSetStatisticsRepository = scope.ServiceProvider.GetRequiredService<IDataSetStatisticsRepository>();

        Func<Task> errorAction = async () => await dataSetStatisticsRepository.SetError(dataSet.Id, nameof(DataSetStatistics_NotFound_Error), cancellationToken);

        await errorAction.Should().ThrowAsync<InvalidOperationException>().WithMessage($"DataSetStatistics with id {dataSet.Id} not found");

        scope.Dispose();
    }
}