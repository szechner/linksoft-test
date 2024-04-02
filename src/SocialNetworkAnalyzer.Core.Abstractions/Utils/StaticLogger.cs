using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SocialNetworkAnalyzer.Core.Abstractions.Utils;

/// <summary>
/// <see cref="ILogger{TCategoryName}"/> factory
/// </summary>
public static class StaticLogger
{
    public static void Initialize(IServiceProvider serviceProvider) => loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

    private static ILoggerFactory? loggerFactory;

    public static ILoggerFactory LoggerFactory
    {
        get
        {
            if(loggerFactory == null)
            {
                throw new InvalidOperationException("LoggerFactory is not initialized");
            }

            return loggerFactory;
        }
    }

    public static ILogger CreateLogger<T>() => LoggerFactory.CreateLogger<T>();
    public static ILogger CreateLogger(string categoryName) => LoggerFactory.CreateLogger(categoryName);
}