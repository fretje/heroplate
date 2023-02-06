using Heroplate.Api.Application.Common.Caching;
using Heroplate.Api.Contracts.Multitenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Heroplate.Api.Infrastructure.Multitenancy;

internal class TenantCache : ITenantCache
{
    private readonly ICacheService _cache;
    private readonly ICacheKeyService _cacheKeys;
    private readonly TenantDbContext _tenantDb;
    private readonly IConfiguration _config;
    public TenantCache(ICacheService cache, ICacheKeyService cacheKeys, TenantDbContext tenantDb, IConfiguration config) =>
        (_cache, _cacheKeys, _tenantDb, _config) = (cache, cacheKeys, tenantDb, config);

    public Task<HeroTenantInfo?> GetByIdAsync(string id, CancellationToken ct) =>
        _cache.GetOrSetAsync(
            GetByIdCacheKey(id),
            () => _tenantDb.TenantInfo.FindAsync(id).AsTask(),
            ct: ct);

    public Task InvalidateByIdCacheAsync(string id, CancellationToken ct) =>
        _cache.RemoveAsync(GetByIdCacheKey(id), ct);

    private string GetByIdCacheKey(string id) =>
        _cacheKeys.GetCacheKey("TenantById", id, false);

    public Task<HeroTenantInfo?> GetByIssuerAsync(string issuer, CancellationToken ct) =>
        issuer == _config["SecuritySettings:AzureAd:RootIssuer"]
            ? GetByIdAsync(MultitenancyConstants.Root.Id, ct)
            : _cache.GetOrSetAsync(
                _cacheKeys.GetCacheKey("TenantByIssuer", issuer, false),
                () => _tenantDb.TenantInfo.FirstOrDefaultAsync(t => t.Issuer == issuer, ct),
                ct: ct);

    public Task InvalidateByIssuerCacheAsync(string issuer, CancellationToken ct) =>
        _cache.RemoveAsync(GetByIssuerCacheKey(issuer), ct);

    private string GetByIssuerCacheKey(string issuer) =>
        _cacheKeys.GetCacheKey("TenantByIssuer", issuer, false);
}