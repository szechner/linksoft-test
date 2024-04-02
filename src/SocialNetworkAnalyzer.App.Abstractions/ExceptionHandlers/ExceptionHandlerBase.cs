using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using SocialNetworkAnalyzer.Data.Abstraction;

namespace SocialNetworkAnalyzer.App.Abstractions.ExceptionHandlers;

/// <summary>
/// Base class for exception handlers
/// </summary>
public abstract class ExceptionHandlerBase
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Do database rollback and writes a problem details response 
    /// </summary>
    protected static async Task WriteProblemDetails(HttpContext httpContext, int statusCode, string title, string message, IDictionary<string, object?>? extensions = null)
    {
        httpContext.RequestServices.GetRequiredService<ITransactionManager>().Rollback();

        httpContext.Response.ContentType = "application/problem+json";
        httpContext.Response.StatusCode = statusCode;
        await JsonSerializer.SerializeAsync(httpContext.Response.Body, new ProblemDetails
        {
            Title = title,
            Detail = message,
            Status = statusCode,
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Instance = httpContext.Request.Path,
            Extensions = extensions ?? new Dictionary<string, object?>()
        }, JsonSerializerOptions);
    }
}