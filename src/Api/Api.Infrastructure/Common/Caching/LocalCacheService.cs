using Heroplate.Api.Application.Common.Caching;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Heroplate.Api.Infrastructure.Common.Caching;

public class LocalCacheService : ICacheService
{
    private readonly ILogger<LocalCacheService> _logger;
    private readonly IMemoryCache _cache;

    public LocalCacheService(IMemoryCache cache, ILogger<LocalCacheService> logger) =>
        (_cache, _logger) = (cache, logger);

    public T? Get<T>(string key) =>
        _cache.Get<T>(key);

    public Task<T?> GetAsync<T>(string key, CancellationToken ct = default) =>
        Task.FromResult(Get<T>(key));

    public void Set<T>(string key, T value, TimeSpan? slidingExpiration = null)
    {
        // TODO: add to appsettings?
        slidingExpiration ??= TimeSpan.FromMinutes(10); // Default expiration time of 10 minutes.

        _cache.Set(key, value, new MemoryCacheEntryOptions { SlidingExpiration = slidingExpiration });
        _logger.LogDebug("Added to Cache : {Key}", key);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? slidingExpiration = null, CancellationToken ct = default)
    {
        Set(key, value, slidingExpiration);
        return Task.CompletedTask;
    }

    public void Refresh(string key) =>
        _cache.TryGetValue(key, out _);

    public Task RefreshAsync(string key, CancellationToken ct = default)
    {
        Refresh(key);
        return Task.CompletedTask;
    }

    public void Remove(string key) =>
        _cache.Remove(key);

    public Task RemoveAsync(string key, CancellationToken ct = default)
    {
        Remove(key);
        return Task.CompletedTask;
    }

    public bool Contains(string key) =>
        _cache.TryGetValue(key, out object _);

    public Task<bool> ContainsAsync(string key, CancellationToken ct = default) =>
        Task.FromResult(Contains(key));
}