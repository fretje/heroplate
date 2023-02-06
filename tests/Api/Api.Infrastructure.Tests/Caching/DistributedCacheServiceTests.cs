using Heroplate.Api.Infrastructure.Common.Caching;

namespace Api.Infrastructure.Tests.Caching;

public class DistributedCacheServiceTests : CacheServiceTests
{
    public DistributedCacheServiceTests(DistributedCacheService cacheService)
        : base(cacheService)
    {
    }
}