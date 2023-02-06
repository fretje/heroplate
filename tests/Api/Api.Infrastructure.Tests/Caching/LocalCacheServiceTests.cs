using Heroplate.Api.Infrastructure.Common.Caching;

namespace Api.Infrastructure.Tests.Caching;

public class LocalCacheServiceTests : CacheServiceTests
{
    public LocalCacheServiceTests(LocalCacheService cacheService)
        : base(cacheService)
    {
    }
}