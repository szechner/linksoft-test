using MediatR;

namespace SocialNetworkAnalyzer.App.Abstractions;

/// <summary>
/// Interface for command handlers which return an <see cref="IEvent"/>
/// </summary>
public interface ICommandRequestHandler<in TCommand, TEvent> : IRequestHandler<TCommand, TEvent>
    where TCommand : ICommand<TEvent>
    where TEvent : IEvent;