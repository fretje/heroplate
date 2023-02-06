using Heroplate.Api.Contracts.Events;

namespace Heroplate.Api.Domain.Abstractions.Events;

public abstract class DomainEvent : IEvent
{
    public DateTime TriggeredOn { get; protected set; } = DateTime.UtcNow;
}