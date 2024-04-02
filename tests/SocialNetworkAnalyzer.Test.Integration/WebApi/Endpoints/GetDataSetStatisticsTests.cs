using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using SocialNetworkAnalyzer.App.Abstractions.Base;
using SocialNetworkAnalyzer.App.DataSet.GetDataSets;
using SocialNetworkAnalyzer.Data.Abstraction;
using SocialNetworkAnalyzer.Data.Model.Database;
using SocialNetworkAnalyzer.Data.Repositories;
using SocialNetworkAnalyzer.Test.Integration.EntityFramework.Extensions;

namespace SocialNetworkAnalyzer.Test.Integration.WebApi.Endpoints;

[TestFixture]
public class GetDataSetStatisticsTests
{
    private HttpClient client;
    private WebApiFactory webApiFactory;

    [OneTimeSetUp]
    public void Setup()
    {
        webApiFactory = new WebApiFactory();
        client = webApiFactory.CreateClient();
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        client.Dispose();
        webApiFactory.Dispose();
    }

    [Test]
    public async Task Get_DataSetStatistics_Return_Result()
    {
        var scope = webApiFactory.Services.CreateScope();

        await RepositoryTestsExtensions.CleanupTable(typeof(DataSetStatistics), scope.ServiceProvider, CancellationToken.None);
        scope.Dispose();

        var response = await client.GetAsync("/datasets");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var resultString = await response.Content.ReadAsStringAsync();

        var result = JsonSerializer.Deserialize<PagedQueryResult<GetDataSetStatisticsQueryResultModel>>(resultString);

        result!.Data.Should().BeEmpty();
        result.Page.Should().Be(0);
        result.PageSize.Should().Be(10);
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
        result.HasNextPage.Should().BeFalse();
        result.HasPreviousPage.Should().BeFalse();

        scope = webApiFactory.Services.CreateScope();
        var dataSetsRepository = scope.ServiceProvider.GetRequiredService<IDataSetsRepository>();
        var dataSetStatisticsRepository = scope.ServiceProvider.GetRequiredService<IDataSetStatisticsRepository>();
        var transactionManager = scope.ServiceProvider.GetRequiredService<ITransactionManager>();
        
        for (int i = 0; i < 10; i++)
        {
            var dataSet = await dataSetsRepository.CreateDataSet($"Test{i}", CancellationToken.None);
            _ = await dataSetStatisticsRepository.CreateDataSetStatistics(dataSet.Id, CancellationToken.None);
        }
        
        transactionManager.Commit();
        scope.Dispose();
        
        response = await client.GetAsync("/datasets");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        resultString = await response.Content.ReadAsStringAsync();

        result = JsonSerializer.Deserialize<PagedQueryResult<GetDataSetStatisticsQueryResultModel>>(resultString);

        result!.Data.Should().HaveCount(10);
        result.Page.Should().Be(0);
        result.PageSize.Should().Be(10);
        result.TotalCount.Should().Be(10);
        result.TotalPages.Should().Be(1);
        result.HasNextPage.Should().BeFalse();
        result.HasPreviousPage.Should().BeFalse();

        result.Data.First().Name.Should().Be("Test0");
        
        response = await client.GetAsync("/datasets?page=0&pageSize=5");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        resultString = await response.Content.ReadAsStringAsync();

        result = JsonSerializer.Deserialize<PagedQueryResult<GetDataSetStatisticsQueryResultModel>>(resultString);

        result!.Data.Should().HaveCount(5);
        result.Page.Should().Be(0);
        result.PageSize.Should().Be(5);
        result.TotalCount.Should().Be(10);
        result.TotalPages.Should().Be(2);
        result.HasNextPage.Should().BeTrue();
        result.HasPreviousPage.Should().BeFalse();

        result.Data.First().Name.Should().Be("Test0");
        
        response = await client.GetAsync("/datasets?page=1&pageSize=5");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        resultString = await response.Content.ReadAsStringAsync();

        result = JsonSerializer.Deserialize<PagedQueryResult<GetDataSetStatisticsQueryResultModel>>(resultString);

        result!.Data.Should().HaveCount(5);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(5);
        result.TotalCount.Should().Be(10);
        result.TotalPages.Should().Be(2);
        result.HasNextPage.Should().BeFalse();
        result.HasPreviousPage.Should().BeTrue();

        result.Data.First().Name.Should().Be("Test5");
        
        response = await client.GetAsync("/datasets?page=2&pageSize=5");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        resultString = await response.Content.ReadAsStringAsync();

        result = JsonSerializer.Deserialize<PagedQueryResult<GetDataSetStatisticsQueryResultModel>>(resultString);

        result!.Data.Should().BeEmpty();
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(5);
        result.TotalCount.Should().Be(10);
        result.TotalPages.Should().Be(2);
        result.HasNextPage.Should().BeFalse();
        result.HasPreviousPage.Should().BeTrue();
    }

    [Test]
    public async Task Get_DataSetStatistics_Validation_Errors()
    {
        var response = await client.GetAsync("/datasets?page=-1");
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var resultString = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ProblemDetails>(resultString);
        result.Should().NotBeNull();
        result!.Title.Should().Be("Validation error");
        
        response = await client.GetAsync("/datasets?pageSize=0");
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        resultString = await response.Content.ReadAsStringAsync();
        result = JsonSerializer.Deserialize<ProblemDetails>(resultString);
        result.Should().NotBeNull();
        result!.Title.Should().Be("Validation error");
    }
}