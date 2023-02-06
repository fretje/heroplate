using Heroplate.Api.Contracts.Notifications;

namespace Heroplate.Api.Application.Common.Interfaces;

public interface INotificationSender : ITransientService
{
    Task BroadcastAsync(INotificationMessage notification, CancellationToken ct);
    Task BroadcastAsync(INotificationMessage notification, IEnumerable<string> excludedConnectionIds, CancellationToken ct);

    Task SendToAllAsync(INotificationMessage notification, CancellationToken ct);
    Task SendToAllAsync(INotificationMessage notification, IEnumerable<string> excludedConnectionIds, CancellationToken ct);
    Task SendToGroupAsync(INotificationMessage notification, string group, CancellationToken ct);
    Task SendToGroupAsync(INotificationMessage notification, string group, IEnumerable<string> excludedConnectionIds, CancellationToken ct);
    Task SendToGroupsAsync(INotificationMessage notification, IEnumerable<string> groupNames, CancellationToken ct);
    Task SendToUserAsync(INotificationMessage notification, string userId, CancellationToken ct);
    Task SendToCurrentUserAsync(INotificationMessage notification, CancellationToken ct);
    Task SendToUsersAsync(INotificationMessage notification, IEnumerable<string> userIds, CancellationToken ct);
}