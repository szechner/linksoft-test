namespace SocialNetworkAnalyzer.App.Abstractions.Base;

/// <summary>
/// Wrapper for paged queries
/// </summary>
public record PagedQuery<TResult>(int Page, int PageSize) : IPagedQuery<TResult>
    where TResult : IPagedQueryResultModel;