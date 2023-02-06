using Heroplate.Api.Application.Common.Interfaces;

namespace Heroplate.Api.Infrastructure.Identity;

internal interface IUserCache : ITransientService
{
    Task<ApplicationUser?> GetByObjectIdAsync(string objectId, CancellationToken ct = default);
    Task InvalidateByObjectIdCacheAsync(string objectId, CancellationToken ct);

    Task<string[]> GetPermissionsAsync(string userId, CancellationToken ct = default);
    Task InvalidatePermissionCacheAsync(string userId, CancellationToken ct);
}