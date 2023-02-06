namespace Heroplate.Api.Application.Common.Events;

public interface IEventPublisher : ITransientService
{
    Task PublishAsync(IEvent @event, CancellationToken ct);
}