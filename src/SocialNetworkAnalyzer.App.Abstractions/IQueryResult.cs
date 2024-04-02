using MediatR;
using SocialNetworkAnalyzer.App.Abstractions.Base;

namespace SocialNetworkAnalyzer.App.Abstractions;

/// <summary>
/// Interface for queries results
/// </summary>
public interface IQueryResult : INotification;

/// <summary>
/// Interface for paged queries result model
/// </summary>
public interface IPagedQueryResultModel;

/// <summary>
/// Interface for paged queries results which returns an array of <see cref="IPagedQueryResultModel"/>
/// </summary>
public interface IPagedQueryResult<T> : IQueryResult, IPaged
    where T : IPagedQueryResultModel
{
    T[] Data { get; set; }

    int TotalCount { get; set; }
    int TotalPages { get; }
    bool HasPreviousPage { get; }
    bool HasNextPage { get; }
}