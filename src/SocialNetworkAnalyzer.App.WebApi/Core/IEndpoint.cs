namespace SocialNetworkAnalyzer.App.WebApi.Core;

/// <summary>
/// Interface for endpoints
/// </summary>
public interface IEndpoint
{
    /// <summary>
    /// Map endpoint to the <see cref="WebApplication"/>
    /// </summary>
    void MapEndpoint(IEndpointRouteBuilder app);
}