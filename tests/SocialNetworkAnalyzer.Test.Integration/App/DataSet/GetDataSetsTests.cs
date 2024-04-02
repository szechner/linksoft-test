using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SocialNetworkAnalyzer.App.DataSet.GetDataSets;
using SocialNetworkAnalyzer.Core.Abstractions.Utils;
using SocialNetworkAnalyzer.Data.Abstraction;
using SocialNetworkAnalyzer.Data.EntityFramework.Contexts;
using SocialNetworkAnalyzer.Data.Repositories;
using SocialNetworkAnalyzer.Test.Integration.EntityFramework.Extensions;

namespace SocialNetworkAnalyzer.Test.Integration.App.DataSet;

[TestFixture]
public class GetDataSetsTests
{
    private ServiceProvider serviceProvider = null!;
    private ServiceProvider serviceProvider2 = null!;

    [OneTimeSetUp]
    public async Task Setup()
    {
        var services = RepositoryTestsExtensions.CreateServiceCollection(nameof(GetDataSetsTests).ToLower());
        var services2 = RepositoryTestsExtensions.CreateServiceCollection($"{nameof(GetDataSetsTests).ToLower()}2");

        serviceProvider = services.BuildServiceProvider();
        serviceProvider2 = services2.BuildServiceProvider();
        StaticLogger.Initialize(serviceProvider);
        StaticLogger.Initialize(serviceProvider2);

        using var scope = serviceProvider.CreateScope();
        await RepositoryTestsExtensions.CleanupTable(typeof(Data.Model.Database.DataSet), serviceProvider, CancellationToken.None);
        await RepositoryTestsExtensions.CleanupTable(typeof(Data.Model.Database.DataSetStatistics), serviceProvider, CancellationToken.None);
        await RepositoryTestsExtensions.CleanupTable(typeof(Data.Model.Database.Relationship), serviceProvider, CancellationToken.None);
        await RepositoryTestsExtensions.CleanupTable(typeof(Data.Model.Database.User), serviceProvider, CancellationToken.None);
        
        using var scope2 = serviceProvider.CreateScope();
        await RepositoryTestsExtensions.CleanupTable(typeof(Data.Model.Database.DataSet), serviceProvider2, CancellationToken.None);
        await RepositoryTestsExtensions.CleanupTable(typeof(Data.Model.Database.DataSetStatistics), serviceProvider2, CancellationToken.None);
        await RepositoryTestsExtensions.CleanupTable(typeof(Data.Model.Database.Relationship), serviceProvider2, CancellationToken.None);
        await RepositoryTestsExtensions.CleanupTable(typeof(Data.Model.Database.User), serviceProvider2, CancellationToken.None);
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
    
    [Test]
    [CancelAfter(90_000)]
    public async Task GetDataSetStatisticsQuery_Returns_Data(CancellationToken cancellationToken)
    {
        var scope = serviceProvider.CreateScope();

        var dataSetStatisticsRepository = scope.ServiceProvider.GetRequiredService<IDataSetStatisticsRepository>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<GetDataSetStatisticsQueryHandler>>();

        for (int i = 0; i < 10; i++)
        {
            var dataSet = await CreateDataSet($"CreateDataSetStatistics{i}", scope);
            _ = await dataSetStatisticsRepository.CreateDataSetStatistics(dataSet.Id, cancellationToken);
        }

        var handler = new GetDataSetStatisticsQueryHandler(dataSetStatisticsRepository, logger);

        var result = await handler.Handle(new GetDataSetStatisticsQuery(0,5), cancellationToken);

        result.Should().NotBeNull();
        result.Data.Length.Should().Be(5);
        result.PageSize.Should().Be(5);
        result.Page.Should().Be(0);
        result.TotalCount.Should().Be(10);
        result.TotalPages.Should().Be(2);
        result.HasNextPage.Should().BeTrue();
        result.HasPreviousPage.Should().BeFalse();
        
        var resultNext = await handler.Handle(new GetDataSetStatisticsQuery(1,5), cancellationToken);

        resultNext.Should().NotBeNull();
        resultNext.Data.Length.Should().Be(5);
        resultNext.PageSize.Should().Be(5);
        resultNext.Page.Should().Be(1);
        resultNext.TotalCount.Should().Be(10);
        resultNext.TotalPages.Should().Be(2);
        resultNext.HasNextPage.Should().BeFalse();
        resultNext.HasPreviousPage.Should().BeTrue();
        
        var resultNext2 = await handler.Handle(new GetDataSetStatisticsQuery(2,5), cancellationToken);

        resultNext2.Should().NotBeNull();
        resultNext2.Data.Length.Should().Be(0);
        resultNext2.PageSize.Should().Be(5);
        resultNext2.Page.Should().Be(2);
        resultNext2.TotalCount.Should().Be(10);
        resultNext2.TotalPages.Should().Be(2);
        resultNext2.HasNextPage.Should().BeFalse();
        resultNext2.HasPreviousPage.Should().BeTrue();
    }
    
    [Test]
    [CancelAfter(90_000)]
    public async Task GetDataSetStatisticsQueryHandler_Validation_Error(CancellationToken cancellationToken)
    {
        var scope = serviceProvider.CreateScope();

        var dataSetStatisticsRepository = scope.ServiceProvider.GetRequiredService<IDataSetStatisticsRepository>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<GetDataSetStatisticsQueryHandler>>();

        var handler = new GetDataSetStatisticsQueryHandler(dataSetStatisticsRepository, logger);

        Func<Task> result = () => handler.Handle(new GetDataSetStatisticsQuery(0,0), cancellationToken);

        result.Should().NotBeNull();
        await result.Should().ThrowAsync<ValidationException>().Where(e =>
            e.Message.Contains("PageSize") &&
            e.Errors.Any(er => er.PropertyName == "PageSize")
        );
        
        result = () => handler.Handle(new GetDataSetStatisticsQuery(-1,10), cancellationToken);

        result.Should().NotBeNull();
        await result.Should().ThrowAsync<ValidationException>().Where(e =>
            e.Message.Contains("Page") &&
            e.Errors.Any(er => er.PropertyName == "Page")
        );
    }
    
    [Test]
    [CancelAfter(90_000)]
    public async Task GetDataSetStatisticsQueryHandler_Returns_Empty(CancellationToken cancellationToken)
    {
        var scope = serviceProvider2.CreateScope();

        var dataSetStatisticsRepository = scope.ServiceProvider.GetRequiredService<IDataSetStatisticsRepository>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<GetDataSetStatisticsQueryHandler>>();

        var handler = new GetDataSetStatisticsQueryHandler(dataSetStatisticsRepository, logger);

        var result = await handler.Handle(new GetDataSetStatisticsQuery(0,5), cancellationToken);

        result.Should().NotBeNull();
        result.Data.Length.Should().Be(0);
        result.PageSize.Should().Be(5);
        result.Page.Should().Be(0);
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
        result.HasNextPage.Should().BeFalse();
        result.HasPreviousPage.Should().BeFalse();
    }
    
    private async Task<Data.Model.Database.DataSet> CreateDataSet(string name, IServiceScope serviceScope)
    {
        var dataSetsRepository = serviceScope.ServiceProvider.GetRequiredService<IDataSetsRepository>();

        var dataSet = await dataSetsRepository.CreateDataSet(name, default);

        var transactionManager = serviceScope.ServiceProvider.GetRequiredService<ITransactionManager>();
        transactionManager.Commit();

        return dataSet;
    }
}