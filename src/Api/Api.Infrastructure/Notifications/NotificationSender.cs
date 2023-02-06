using Finbuckle.MultiTenant;
using Heroplate.Api.Application.Common.Interfaces;
using Heroplate.Api.Contracts.Notifications;
using Microsoft.AspNetCore.SignalR;
using static Heroplate.Api.Contracts.Notifications.NotificationConstants;

namespace Heroplate.Api.Infrastructure.Notifications;

internal class NotificationSender : INotificationSender
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ITenantInfo _currentTenant;
    private readonly ICurrentUser _currentUser;
    public NotificationSender(IHubContext<NotificationHub> hubContext, ITenantInfo currentTenant, ICurrentUser currentUser) =>
        (_hubContext, _currentTenant, _currentUser) = (hubContext, currentTenant, currentUser);

    public Task BroadcastAsync(INotificationMessage notification, CancellationToken ct)
    {
        // calling sendasync with a cancelled cancellation token results in a disconnect (Failed writing message. Aborting connection.)
        ct.ThrowIfCancellationRequested();
        return _hubContext.Clients.All
            .SendAsync(NotificationFromServer, notification.GetType().FullName, notification, ct);
    }

    public Task BroadcastAsync(INotificationMessage notification, IEnumerable<string> excludedConnectionIds, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        return _hubContext.Clients.AllExcept(excludedConnectionIds)
            .SendAsync(NotificationFromServer, notification.GetType().FullName, notification, ct);
    }

    public Task SendToAllAsync(INotificationMessage notification, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        return _hubContext.Clients.Group($"GroupTenant-{_currentTenant.Id}")
            .SendAsync(NotificationFromServer, notification.GetType().FullName, notification, ct);
    }

    public Task SendToAllAsync(INotificationMessage notification, IEnumerable<string> excludedConnectionIds, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        return _hubContext.Clients.GroupExcept($"GroupTenant-{_currentTenant.Id}", excludedConnectionIds)
            .SendAsync(NotificationFromServer, notification.GetType().FullName, notification, ct);
    }

    public Task SendToGroupAsync(INotificationMessage notification, string group, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        return _hubContext.Clients.Group(group)
            .SendAsync(NotificationFromServer, notification.GetType().FullName, notification, ct);
    }

    public Task SendToGroupAsync(INotificationMessage notification, string group, IEnumerable<string> excludedConnectionIds, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        return _hubContext.Clients.GroupExcept(group, excludedConnectionIds)
            .SendAsync(NotificationFromServer, notification.GetType().FullName, notification, ct);
    }

    public Task SendToGroupsAsync(INotificationMessage notification, IEnumerable<string> groupNames, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        return _hubContext.Clients.Groups(groupNames)
            .SendAsync(NotificationFromServer, notification.GetType().FullName, notification, ct);
    }

    public Task SendToUserAsync(INotificationMessage notification, string userId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        return _hubContext.Clients.User(userId)
            .SendAsync(NotificationFromServer, notification.GetType().FullName, notification, ct);
    }

    public Task SendToCurrentUserAsync(INotificationMessage notification, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        return _hubContext.Clients.User(_currentUser.GetUserId().ToString())
            .SendAsync(NotificationFromServer, notification.GetType().FullName, notification, ct);
    }

    public Task SendToUsersAsync(INotificationMessage notification, IEnumerable<string> userIds, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        return _hubContext.Clients.Users(userIds)
            .SendAsync(NotificationFromServer, notification.GetType().FullName, notification, ct);
    }
}