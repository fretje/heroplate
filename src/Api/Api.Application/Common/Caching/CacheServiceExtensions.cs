namespace Heroplate.Api.Application.Common.Caching;

public static class CacheServiceExtensions
{
    public static T? GetOrSet<T>(this ICacheService cache, string key, Func<T?> getItemCallback, TimeSpan? slidingExpiration = null)
    {
        var value = cache.Get<T>(key);

        if (value is not null)
        {
            return value;
        }

        value = getItemCallback();

        if (value is not null)
        {
            cache.Set(key, value, slidingExpiration);
        }

        return value;
    }

    public static async Task<T?> GetOrSetAsync<T>(this ICacheService cache, string key, Func<Task<T>> getItemCallback, TimeSpan? slidingExpiration = null, CancellationToken ct = default)
    {
        var value = await cache.GetAsync<T>(key, ct);

        if (value is not null)
        {
            return value;
        }

        value = await getItemCallback();

        if (value is not null)
        {
            await cache.SetAsync(key, value, slidingExpiration, ct);
        }

        return value;
    }
}