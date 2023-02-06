using Heroplate.Api.Application.Common.Interfaces;

namespace Heroplate.Api.Infrastructure.Multitenancy;

public interface ITenantCache : ITransientService
{
    Task<HeroTenantInfo?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<HeroTenantInfo?> GetByIssuerAsync(string issuer, CancellationToken ct = default);
    Task InvalidateByIdCacheAsync(string id, CancellationToken ct);
    Task InvalidateByIssuerCacheAsync(string issuer, CancellationToken ct);
}