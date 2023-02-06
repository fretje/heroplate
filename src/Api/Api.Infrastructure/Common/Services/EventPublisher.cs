using Heroplate.Api.Application.Common.Events;
using Heroplate.Api.Contracts.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Heroplate.Api.Infrastructure.Common.Services;

public class EventPublisher : IEventPublisher
{
    private readonly ILogger<EventPublisher> _logger;
    private readonly IPublisher _mediator;

    public EventPublisher(ILogger<EventPublisher> logger, IPublisher mediator) =>
        (_logger, _mediator) = (logger, mediator);

    public Task PublishAsync(IEvent @event, CancellationToken ct)
    {
        _logger.LogInformation("Publishing Event : {Event}", @event.GetType().Name);
        return _mediator.Publish(CreateEventNotification(@event), ct);
    }

    private static INotification CreateEventNotification(IEvent @event) =>
        (INotification)Activator.CreateInstance(
            typeof(EventNotification<>).MakeGenericType(@event.GetType()), @event)!;
}