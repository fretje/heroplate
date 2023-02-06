using Heroplate.Api.Contracts.Notifications;

namespace Heroplate.Admin.Infrastructure.Notifications;

public interface INotificationPublisher
{
    Task PublishAsync(INotificationMessage notification);
}