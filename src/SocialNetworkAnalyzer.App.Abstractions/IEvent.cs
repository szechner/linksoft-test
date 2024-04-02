using MediatR;

namespace SocialNetworkAnalyzer.App.Abstractions;

/// <summary>
/// Interface for events that are raised by <see cref="ICommand{TResult}"/>
/// </summary>
public interface IEvent : INotification;