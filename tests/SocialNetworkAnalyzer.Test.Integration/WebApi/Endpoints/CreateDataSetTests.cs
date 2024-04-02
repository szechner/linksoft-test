using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using SocialNetworkAnalyzer.App.Events;

namespace SocialNetworkAnalyzer.Test.Integration.WebApi.Endpoints;

[TestFixture]
public class CreateDataSetTests
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
    public async Task Create_DataSet()
    {
        var content = new MultipartFormDataContent();

        var dataSetNameContent = new StringContent("TestDataSet", Encoding.UTF8, "application/json");
        content.Add(dataSetNameContent, "dataSetName");

        var sb = new StringBuilder();
        sb.AppendLine("1 2");
        sb.AppendLine("2 3");
        sb.AppendLine("3 4");
        sb.AppendLine("4 1");
        
        var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes(sb.ToString()));
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
        content.Add(fileContent, "file", "file.txt");
        
        var response = await client.PostAsync("/datasets", content);
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var resultString = await response.Content.ReadAsStringAsync();
        
        var result = JsonSerializer.Deserialize<DataSetCreatedEvent>(resultString);
        result.Should().NotBeNull();
        result!.DataSetId.Should().BeGreaterThan(0);
    }
    
    [Test]
    public async Task Create_DataSet_Validation_Errors_Empty_File()
    {
        var content = new MultipartFormDataContent();

        var dataSetNameContent = new StringContent("TestDataSet", Encoding.UTF8, "application/json");
        content.Add(dataSetNameContent, "dataSetName");
        
        var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes(string.Empty));
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
        content.Add(fileContent, "file", "file.txt");
        
        var response = await client.PostAsync("/datasets", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var resultString = await response.Content.ReadAsStringAsync();
        
        var result = JsonSerializer.Deserialize<ProblemDetails>(resultString);
        result.Should().NotBeNull();
        result!.Title.Should().Be("Validation error");
    }
    
    [Test]
    public async Task Create_DataSet_Validation_Errors_Empty_Not_Present()
    {
        var content = new MultipartFormDataContent();

        var sb = new StringBuilder();
        sb.AppendLine("1 2");
        sb.AppendLine("2 3");
        sb.AppendLine("3 4");
        sb.AppendLine("4 1");
        
        var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes(sb.ToString()));
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
        content.Add(fileContent, "file", "file.txt");
        
        var response = await client.PostAsync("/datasets", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Test]
    public async Task Create_DataSet_Validation_Errors_Empty_Name()
    {
        var content = new MultipartFormDataContent();

        var dataSetNameContent = new StringContent(string.Empty, Encoding.UTF8, "application/json");
        content.Add(dataSetNameContent, "dataSetName");
        
        var sb = new StringBuilder();
        sb.AppendLine("1 2");
        sb.AppendLine("2 3");
        sb.AppendLine("3 4");
        sb.AppendLine("4 1");
        
        var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes(sb.ToString()));
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
        content.Add(fileContent, "file", "file.txt");
        
        var response = await client.PostAsync("/datasets", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var resultString = await response.Content.ReadAsStringAsync();
        
        var result = JsonSerializer.Deserialize<ProblemDetails>(resultString);
        result.Should().NotBeNull();
        result!.Title.Should().Be("Validation error");
    }
}