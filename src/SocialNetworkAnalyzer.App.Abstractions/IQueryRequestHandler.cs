using MediatR;

namespace SocialNetworkAnalyzer.App.Abstractions;

/// <summary>
/// Interface for query handlers which return an <see cref="IQueryResult"/>
/// </summary>
public interface IQueryRequestHandler<in TQuery, TQueryResult> : IRequestHandler<TQuery, TQueryResult>
    where TQuery : IQuery<TQueryResult>
    where TQueryResult : IQueryResult;

/// <summary>
/// Interface for paged query handlers which return an <see cref="IPagedQueryResult{TQueryResult}"/> 
/// </summary>
public interface IPagedQueryRequestHandler<in TQuery, TQueryResult> : IRequestHandler<TQuery, IPagedQueryResult<TQueryResult>>
    where TQuery : IPagedQuery<TQueryResult>
    where TQueryResult : IPagedQueryResultModel;