using MediatR;

namespace SocialNetworkAnalyzer.App.Abstractions;

/// <summary>
/// Interface for event handlers to handle <see cref="IEvent"/> raised by <see cref="ICommand{TResult}"/> 
/// </summary>
public interface IEventHandler<in T> : INotificationHandler<T>
    where T : IEvent;