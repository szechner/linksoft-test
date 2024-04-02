namespace SocialNetworkAnalyzer.App.Abstractions.Base;

/// <summary>
/// Interface for paged queries
/// </summary>
public interface IPaged
{
    public int Page { get; }
    public int PageSize { get; }
}