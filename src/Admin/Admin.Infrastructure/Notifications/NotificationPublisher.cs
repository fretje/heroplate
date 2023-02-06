using Heroplate.Api.Contracts.Notifications;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Heroplate.Admin.Infrastructure.Notifications;

public class NotificationPublisher : INotificationPublisher
{
    private readonly ILogger<NotificationPublisher> _logger;
    private readonly IPublisher _mediator;

    public NotificationPublisher(ILogger<NotificationPublisher> logger, IPublisher mediator) =>
        (_logger, _mediator) = (logger, mediator);

    public Task PublishAsync(INotificationMessage notification)
    {
        _logger.LogDebug("Publishing Notification : {Name} {Value}", notification.GetType().Name, JsonSerializer.Serialize((object)notification));
        return _mediator.Publish(CreateNotificationWrapper(notification));
    }

    private static INotification CreateNotificationWrapper(INotificationMessage notification) =>
        (INotification)Activator.CreateInstance(
            typeof(NotificationWrapper<>).MakeGenericType(notification.GetType()), notification)!;
}