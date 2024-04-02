using MediatR;

namespace SocialNetworkAnalyzer.App.Abstractions;

/// <summary>
/// Interface for commands which return an <see cref="IEvent"/>
/// </summary>
public interface ICommand<out TResult> : IRequest<TResult> where TResult : IEvent;