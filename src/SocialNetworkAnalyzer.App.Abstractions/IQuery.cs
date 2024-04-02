using MediatR;
using SocialNetworkAnalyzer.App.Abstractions.Base;

namespace SocialNetworkAnalyzer.App.Abstractions;

/// <summary>
/// Interface for queries which return an <see cref="IQueryResult"/>
/// </summary>
public interface IQuery<out TResult> : IRequest<TResult>
    where TResult : IQueryResult;

/// <summary>
/// Interface for queries which return an <see cref="IPagedQueryResult{TResult}"/>
/// </summary>
public interface IPagedQuery<TResult> : IPaged, IQuery<IPagedQueryResult<TResult>>
    where TResult : IPagedQueryResultModel;