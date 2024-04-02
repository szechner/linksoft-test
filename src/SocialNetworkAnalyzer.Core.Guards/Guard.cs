namespace SocialNetworkAnalyzer.Core.Guards;

/// <summary>
/// Interface of the guard
/// </summary>
public interface IGuard;

/// <summary>
/// Guard implementation
/// </summary>
public sealed class Guard : IGuard
{
    private Guard()
    {
    }

    /// <summary>
    /// Factory method to create a new guard
    /// </summary>
    public static IGuard Require => new Guard();
}