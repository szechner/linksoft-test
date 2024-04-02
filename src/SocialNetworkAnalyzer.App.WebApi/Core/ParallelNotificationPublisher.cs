using MediatR;

namespace SocialNetworkAnalyzer.App.WebApi.Core;

/// <summary>
/// Publisher for notifications that executes all handlers in parallel
/// </summary>
public class ParallelNotificationPublisher : INotificationPublisher
{
    public Task Publish(IEnumerable<NotificationHandlerExecutor> handlerExecutors, INotification notification, CancellationToken cancellationToken)
    {
        var tasks = handlerExecutors
            .Select(handler => handler.HandlerCallback(
                notification,
                cancellationToken))
            .ToArray();

        return Task.WhenAll(tasks);
    }
}