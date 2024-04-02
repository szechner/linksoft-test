namespace SocialNetworkAnalyzer.App.WebApi.Core;

/// <summary>
/// Extensions for <see cref="IEndpoint"/>
/// </summary>
public static class EndpointExtensions
{
    /// <summary>
    /// Maps all endpoints in the assembly 
    /// </summary>
    public static void MapEndpoints(this WebApplication app)
    {
        var endpoints = typeof(EndpointExtensions).Assembly.GetTypes()
            .Where(x => x.GetInterfaces().Contains(typeof(IEndpoint)) && x is { IsInterface: false, IsAbstract: false })
            .Select(Activator.CreateInstance)
            .Cast<IEndpoint>();

        endpoints.ToList().ForEach(x => x.MapEndpoint(app));
    }
}