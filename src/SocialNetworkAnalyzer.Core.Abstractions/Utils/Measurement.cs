using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace SocialNetworkAnalyzer.Core.Abstractions.Utils;

/// <summary>
/// Measurement utility class
/// </summary>
public static class Measurement
{
    public static IDisposable ElapsedTime(object caller, [CallerMemberName] string? callerMemberName = null)
    {
        return new MeasurementScope(caller.GetType(), callerMemberName);
    }
}

/// <summary>
/// Internal scope of the measurement
/// </summary>
public class MeasurementScope : IDisposable
{
    private readonly Type caller;
    private readonly string? name;
    private readonly Stopwatch stopwatch;
    private readonly ILogger logger;

    public MeasurementScope(Type caller, string? name)
    {
        this.caller = caller;
        this.name = name;
        stopwatch = Stopwatch.StartNew();
        logger = StaticLogger.CreateLogger(caller.Name);
    }

    public void Dispose()
    {
        stopwatch.Stop();
        logger.LogDebug("*** {Caller}.{Name} took {ElapsedMilliseconds}ms", caller.Name, name, stopwatch.ElapsedMilliseconds);
    }
}