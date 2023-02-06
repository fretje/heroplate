using System.Diagnostics.CodeAnalysis;

namespace Heroplate.Api.Application.Common.Events;

// This is just a shorthand to make it a bit easier to create event handlers for specific events.
public interface IEventNotificationHandler<TEvent> : INotificationHandler<EventNotification<TEvent>>
    where TEvent : IEvent
{
}

public abstract class EventNotificationHandler<TEvent> : INotificationHandler<EventNotification<TEvent>>
    where TEvent : IEvent
{
    public Task Handle(EventNotification<TEvent> notification, CancellationToken ct) =>
        Handle(notification.Event, ct);

    [SuppressMessage("Style", "VSTHRD200:Use \"Async\" suffix for async methods", Justification = "We stay conform with MediatR here")]
    public abstract Task Handle(TEvent @event, CancellationToken ct);
}