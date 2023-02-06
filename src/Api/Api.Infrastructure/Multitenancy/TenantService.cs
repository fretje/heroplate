using Finbuckle.MultiTenant;
using Heroplate.Api.Application.Common.Events;
using Heroplate.Api.Application.Common.Exceptions;
using Heroplate.Api.Application.Common.Persistence;
using Heroplate.Api.Application.Multitenancy;
using Heroplate.Api.Domain.MultiTenancy;
using Heroplate.Api.Infrastructure.Persistence;
using Heroplate.Api.Infrastructure.Persistence.Initialization;
using Mapster;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace Heroplate.Api.Infrastructure.Multitenancy;

internal class TenantService : ITenantService
{
    private readonly IMultiTenantStore<HeroTenantInfo> _tenantStore;
    private readonly IConnectionStringSecurer _csSecurer;
    private readonly IDatabaseInitializer _dbInitializer;
    private readonly IStringLocalizer _t;
    private readonly DatabaseSettings _dbSettings;
    private readonly IEventPublisher _events;
    public TenantService(
        IMultiTenantStore<HeroTenantInfo> tenantStore,
        IConnectionStringSecurer csSecurer,
        IDatabaseInitializer dbInitializer,
        IStringLocalizer<TenantService> localizer,
        IOptions<DatabaseSettings> dbSettings,
        IEventPublisher events)
    {
        _tenantStore = tenantStore;
        _csSecurer = csSecurer;
        _dbInitializer = dbInitializer;
        _t = localizer;
        _dbSettings = dbSettings.Value;
        _events = events;
    }

    public async Task<List<TenantDto>> GetAllAsync()
    {
        var tenants = (await _tenantStore.GetAllAsync()).Adapt<List<TenantDto>>();
        tenants.ForEach(t => t.ConnectionString = _csSecurer.MakeSecure(t.ConnectionString));
        return tenants;
    }

    public async Task<bool> ExistsWithIdAsync(string id) =>
        await _tenantStore.TryGetAsync(id) is not null;

    public async Task<bool> ExistsWithNameAsync(string name) =>
        (await _tenantStore.GetAllAsync()).Any(t => t.Name == name);

    public async Task<TenantDto> GetByIdAsync(string id) =>
        (await GetTenantInfoAsync(id))
            .Adapt<TenantDto>();

    public async Task<string> CreateAsync(CreateTenantRequest req, CancellationToken ct)
    {
        if (req.ConnectionString?.Trim() == _dbSettings.ConnectionString.Trim())
        {
            req.ConnectionString = "";
        }

        var tenant = new HeroTenantInfo(req.Id, req.Name, req.ConnectionString, req.AdminEmail, req.Issuer);
        await _tenantStore.TryAddAsync(tenant);

        // TODO: run this in a hangfire job? will then have to send mail when it's ready or not
        try
        {
            await _dbInitializer.InitializeApplicationDbForTenantAsync(tenant, ct);
        }
        catch
        {
            await _tenantStore.TryRemoveAsync(req.Id);
            throw;
        }

        await _events.PublishAsync(new TenantCreatedEvent(tenant.Id), ct);

        return tenant.Id;
    }

    public async Task<string> ActivateAsync(string id, CancellationToken ct)
    {
        var tenant = await GetTenantInfoAsync(id);

        if (tenant.IsActive)
        {
            throw new ConflictException(_t["Tenant is already Activated."]);
        }

        tenant.Activate();

        await _tenantStore.TryUpdateAsync(tenant);

        await _events.PublishAsync(new TenantUpdatedEvent(tenant.Id, tenant.Issuer), ct);

        return _t["Tenant {0} is now Activated.", id];
    }

    public async Task<string> DeactivateAsync(string id, CancellationToken ct)
    {
        var tenant = await GetTenantInfoAsync(id);

        if (!tenant.IsActive)
        {
            throw new ConflictException(_t["Tenant is already Deactivated."]);
        }

        tenant.Deactivate();

        await _tenantStore.TryUpdateAsync(tenant);

        await _events.PublishAsync(new TenantUpdatedEvent(tenant.Id, tenant.Issuer), ct);

        return _t[$"Tenant {0} is now Deactivated.", id];
    }

    public async Task<string> UpdateSubscriptionAsync(string id, DateTime extendedExpiryDate, CancellationToken ct)
    {
        var tenant = await GetTenantInfoAsync(id);

        tenant.SetValidity(extendedExpiryDate);

        await _tenantStore.TryUpdateAsync(tenant);

        await _events.PublishAsync(new TenantUpdatedEvent(tenant.Id, tenant.Issuer), ct);

        return _t[$"Tenant {0}'s Subscription Upgraded. Now Valid till {1}.", id, tenant.ValidUpto];
    }

    private async Task<HeroTenantInfo> GetTenantInfoAsync(string id) =>
        await _tenantStore.TryGetAsync(id)
            ?? throw new NotFoundException(_t["{0} {1} Not Found.", typeof(HeroTenantInfo).Name, id]);
}