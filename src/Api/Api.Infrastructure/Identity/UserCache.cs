using Heroplate.Api.Application.Common.Caching;
using Heroplate.Api.Application.Identity.Users;
using Heroplate.Shared.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Heroplate.Api.Infrastructure.Identity;

internal class UserCache : IUserCache
{
    private readonly ICacheService _cache;
    private readonly ICacheKeyService _cacheKeys;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserService _userService;
    public UserCache(ICacheService cache, ICacheKeyService cacheKeys, UserManager<ApplicationUser> userManager, IUserService userService) =>
        (_cache, _cacheKeys, _userManager, _userService) = (cache, cacheKeys, userManager, userService);

    public Task<ApplicationUser?> GetByObjectIdAsync(string objectId, CancellationToken ct) =>
        _cache.GetOrSetAsync(
            GetByObjectIdCacheKey(objectId),
            () => _userManager.Users.FirstOrDefaultAsync(u => u.ObjectId == objectId),
            ct: ct);

    public Task InvalidateByObjectIdCacheAsync(string objectId, CancellationToken ct) =>
        _cache.RemoveAsync(GetByObjectIdCacheKey(objectId), ct);

    private string GetByObjectIdCacheKey(string objectId) =>
        _cacheKeys.GetCacheKey("UserByObjectId", objectId);

    public async Task<string[]> GetPermissionsAsync(string userId, CancellationToken ct) =>
        await _cache.GetOrSetAsync(
            GetPermissionCacheKey(userId),
            () => _userService.GetPermissionsAsync(userId, ct),
            ct: ct)
                ?? Array.Empty<string>();

    public Task InvalidatePermissionCacheAsync(string userId, CancellationToken ct) =>
        _cache.RemoveAsync(GetPermissionCacheKey(userId), ct);

    private string GetPermissionCacheKey(string userId) =>
        _cacheKeys.GetCacheKey(CustomClaimTypes.Permission, userId);
}