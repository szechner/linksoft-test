using System.Reflection;

namespace SocialNetworkAnalyzer.App;

/// <summary>
/// Helper class to get the assembly SocialNetworkAnalyzer.App
/// </summary>
public static class AppAssembly
{
    public static Assembly Assembly => typeof(AppAssembly).Assembly;
}