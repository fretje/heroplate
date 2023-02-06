using Heroplate.Api.Application.Multitenancy;

namespace Heroplate.Api.Infrastructure.Multitenancy;

internal class CurrentTenantService : ICurrentTenantService
{
    private readonly HeroTenantInfo _currentTenant;
    public CurrentTenantService(HeroTenantInfo currentTenant) => _currentTenant = currentTenant;

    public string Id => _currentTenant.Id;
    public string Identifier => _currentTenant.Identifier;
}