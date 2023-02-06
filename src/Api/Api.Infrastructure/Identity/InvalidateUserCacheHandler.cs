using Heroplate.Api.Application.Common.Events;
using Heroplate.Api.Domain.Identity;
using Microsoft.AspNetCore.Identity;

namespace Heroplate.Api.Infrastructure.Identity;

internal class InvalidateUserCacheHandler :
    IEventNotificationHandler<ApplicationUserUpdatedEvent>,
    IEventNotificationHandler<ApplicationRoleUpdatedEvent>
{
    private readonly IUserCache _userCache;
    private readonly UserManager<ApplicationUser> _userManager;

    public InvalidateUserCacheHandler(IUserCache userCache, UserManager<ApplicationUser> userManager) =>
        (_userCache, _userManager) = (userCache, userManager);

    public async Task Handle(EventNotification<ApplicationUserUpdatedEvent> notification, CancellationToken ct)
    {
        if (notification.Event.ObjectId is not null)
        {
            await _userCache.InvalidateByObjectIdCacheAsync(notification.Event.ObjectId, ct);
        }

        if (notification.Event.RolesUpdated)
        {
            await _userCache.InvalidatePermissionCacheAsync(notification.Event.UserId, ct);
        }
    }

    public async Task Handle(EventNotification<ApplicationRoleUpdatedEvent> notification, CancellationToken ct)
    {
        if (notification.Event.PermissionsUpdated)
        {
            foreach (var user in await _userManager.GetUsersInRoleAsync(notification.Event.RoleName))
            {
                await _userCache.InvalidatePermissionCacheAsync(user.Id, ct);
            }
        }
    }
}