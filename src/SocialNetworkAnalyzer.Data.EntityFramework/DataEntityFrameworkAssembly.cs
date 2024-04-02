using System.Reflection;

namespace SocialNetworkAnalyzer.Data.EntityFramework;

/// <summary>
/// Helper class to get the assembly SocialNetworkAnalyzer.Data.EntityFramework
/// </summary>
public static class DataEntityFrameworkAssembly
{
    public static Assembly Assembly => typeof(DataEntityFrameworkAssembly).Assembly;
}