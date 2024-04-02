using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace SocialNetworkAnalyzer.App.ExceptionHandlers;

public static class ExceptionHandlerExtensions
{
    /// <summary>
    /// Add all exception handlers to the <see cref="IServiceCollection"/>
    /// </summary>
    public static void AddExceptionHandlers(this IServiceCollection services)
    {
        var exceptionHandlers = typeof(ExceptionHandlerExtensions).Assembly.GetTypes()
            .Where(x => x.GetInterfaces().Contains(typeof(IExceptionHandler)) && x is { IsInterface: false, IsAbstract: false })
            .ToList();

        foreach (var exceptionHandler in exceptionHandlers)
        {
            services.AddSingleton(typeof(IExceptionHandler), exceptionHandler);
        }
    }
}