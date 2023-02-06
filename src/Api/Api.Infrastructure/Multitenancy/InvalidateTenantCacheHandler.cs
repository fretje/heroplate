using Heroplate.Api.Application.Common.Events;
using Heroplate.Api.Domain.MultiTenancy;

namespace Heroplate.Api.Infrastructure.Multitenancy;

internal class InvalidateTenantCacheHandler : IEventNotificationHandler<TenantUpdatedEvent>
{
    private readonly ITenantCache _tenantCache;
    public InvalidateTenantCacheHandler(ITenantCache tenantCache) => _tenantCache = tenantCache;

    public async Task Handle(EventNotification<TenantUpdatedEvent> notification, CancellationToken ct)
    {
        await _tenantCache.InvalidateByIdCacheAsync(notification.Event.TenantId, ct);
        if (!string.IsNullOrWhiteSpace(notification.Event.Issuer))
        {
            await _tenantCache.InvalidateByIssuerCacheAsync(notification.Event.Issuer, ct);
        }
    }
}