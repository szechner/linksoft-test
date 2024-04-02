using System.Text.Json.Serialization;

namespace SocialNetworkAnalyzer.App.Abstractions.Base;

/// <summary>
/// Base wrapper for paged queries results
/// </summary>
public abstract class PagedQueryResult
{
    [JsonConstructor]
    public PagedQueryResult()
    {
    }
    
    [JsonPropertyName("totalCount")]
    public int TotalCount { get; set; }
    
    [JsonPropertyName("page")]
    public int Page { get; set; }
    
    [JsonPropertyName("pageSize")]
    public int PageSize { get; set; }
    
    [JsonPropertyName("totalPages")]
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    
    [JsonPropertyName("hasPreviousPage")]
    public bool HasPreviousPage => Page > 0;
    
    [JsonPropertyName("hasNextPage")]
    public bool HasNextPage => Page + 1 < TotalPages;

    public static PagedQueryResult<T> Empty<T>(int pageSize)
        where T : IPagedQueryResultModel
    {
        return new PagedQueryResult<T>
        {
            Data = Array.Empty<T>(),
            TotalCount = 0,
            PageSize = pageSize,
            Page = 0
        };
    }

    public static PagedQueryResult<T> Create<T>(IEnumerable<T> data, int totalCount, int page, int pageSize)
        where T : IPagedQueryResultModel
    {
        return new PagedQueryResult<T>
        {
            Data = data.ToArray(),
            TotalCount = totalCount,
            PageSize = pageSize,
            Page = page
        };
    }
}

/// <summary>
/// Wrapper for paged queries results 
/// </summary>
public sealed class PagedQueryResult<TModel> : PagedQueryResult, IPagedQueryResult<TModel>
    where TModel : IPagedQueryResultModel
{
    [JsonPropertyName("data")]
    public TModel[] Data { get; set; } = Array.Empty<TModel>();
}