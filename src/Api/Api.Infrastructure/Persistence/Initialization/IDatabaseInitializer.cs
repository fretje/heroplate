using Heroplate.Api.Infrastructure.Multitenancy;

namespace Heroplate.Api.Infrastructure.Persistence.Initialization;

internal interface IDatabaseInitializer
{
    Task<bool> InitializeDatabasesAsync(CancellationToken ct);
    Task<bool> InitializeApplicationDbForTenantAsync(HeroTenantInfo tenant, CancellationToken ct);
}