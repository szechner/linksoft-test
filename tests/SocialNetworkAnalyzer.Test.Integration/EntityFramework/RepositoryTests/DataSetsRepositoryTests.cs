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
[Parallelizable(ParallelScope.All)]
public class DataSetsRepositoryTests
{
    private ServiceProvider serviceProvider = null!;
    
    [OneTimeSetUp]
    public async Task Setup()
    {
        var services = RepositoryTestsExtensions.CreateServiceCollection(nameof(DataSetsRepositoryTests).ToLower());

        serviceProvider = services.BuildServiceProvider();
        StaticLogger.Initialize(serviceProvider);

        using var scope = serviceProvider.CreateScope();
        await RepositoryTestsExtensions.CleanupTable(typeof(DataSet), serviceProvider, CancellationToken.None);
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
    public async Task Add_DataSet_To_Database_With_CreateDataSet(CancellationToken cancellationToken)
    {
        var scope = serviceProvider.CreateScope();
        var dataSetsRepository = scope.ServiceProvider.GetRequiredService<IDataSetsRepository>();

        var dataSet = await dataSetsRepository.CreateDataSet("TestDataSet", cancellationToken);
        
        var transactionManager = scope.ServiceProvider.GetRequiredService<ITransactionManager>();
        transactionManager.Commit();
        scope.Dispose();

        scope = serviceProvider.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<SocialMappingContext>();

        var dataSetsDbSet = context.Set<DataSet>();

        var dbDataSet = await dataSetsDbSet.FindAsync([dataSet.Id], cancellationToken);

        dbDataSet.Should().BeEquivalentTo(dataSet);
        
        scope.Dispose();
    }
}