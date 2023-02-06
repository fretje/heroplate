namespace Heroplate.Api.Application.Common.Caching;

public interface ICacheService
{
    T? Get<T>(string key);
    Task<T?> GetAsync<T>(string key, CancellationToken token = default);

    void Set<T>(string key, T value, TimeSpan? slidingExpiration = null);
    Task SetAsync<T>(string key, T value, TimeSpan? slidingExpiration = null, CancellationToken ct = default);

    void Refresh(string key);
    Task RefreshAsync(string key, CancellationToken token = default);

    void Remove(string key);
    Task RemoveAsync(string key, CancellationToken token = default);

    bool Contains(string key);
    Task<bool> ContainsAsync(string key, CancellationToken ct = default);
}