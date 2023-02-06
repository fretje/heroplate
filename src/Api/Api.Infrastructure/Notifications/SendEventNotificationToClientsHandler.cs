using Heroplate.Api.Application.Common.Events;
using Heroplate.Api.Application.Common.Interfaces;
using Heroplate.Api.Contracts.Notifications;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Heroplate.Api.Infrastructure.Notifications;

// Sends all events that are also an INotificationMessage to all clients
// Note: for this to work, the Event/NotificationMessage class needs to be in the
// shared project (i.e. have the same FullName - so with namespace - on both sides)
public class SendEventNotificationToClientsHandler<TNotification> : INotificationHandler<TNotification>
    where TNotification : INotification
{
    private readonly INotificationSender _notifications;
    private readonly ILogger _logger;

    public SendEventNotificationToClientsHandler(INotificationSender notifications, ILogger<SendEventNotificationToClientsHandler<TNotification>> logger) =>
        (_notifications, _logger) = (notifications, logger);

    public Task Handle(TNotification notification, CancellationToken ct)
    {
        var notificationType = typeof(TNotification);
        if (notificationType.IsGenericType
            && notificationType.GetGenericTypeDefinition() == typeof(EventNotification<>)
            && notificationType.GetGenericArguments()[0] is { } eventType
            && eventType.IsAssignableTo(typeof(INotificationMessage)))
        {
            INotificationMessage notificationMessage = ((dynamic)notification).Event;
            _logger.LogInformation("Sending {EventType} {Event} to all Clients", eventType.Name, notificationMessage);
            return _notifications.SendToAllAsync(notificationMessage, ct);
        }

        return Task.CompletedTask;
    }
}