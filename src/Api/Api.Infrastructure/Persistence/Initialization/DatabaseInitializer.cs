using Finbuckle.MultiTenant;
using Heroplate.Api.Contracts.Multitenancy;
using Heroplate.Api.Infrastructure.Multitenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Heroplate.Api.Infrastructure.Persistence.Initialization;

internal class DatabaseInitializer : IDatabaseInitializer
{
    private readonly TenantDbContext _tenantDbContext;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(TenantDbContext tenantDbContext, IServiceProvider serviceProvider, ILogger<DatabaseInitializer> logger)
    {
        _tenantDbContext = tenantDbContext;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task<bool> InitializeDatabasesAsync(CancellationToken ct)
    {
        bool initialized = await InitializeTenantDbAsync(ct);

        foreach (var tenant in await _tenantDbContext.TenantInfo.ToListAsync(ct))
        {
            if (await InitializeApplicationDbForTenantAsync(tenant, ct))
            {
                initialized = true;
            }
        }

        return initialized;
    }

    public async Task<bool> InitializeApplicationDbForTenantAsync(HeroTenantInfo tenant, CancellationToken ct)
    {
        // First create a new scope
        using var scope = _serviceProvider.CreateScope();

        // Then set current tenant so the right connectionstring is used
        scope.ServiceProvider.GetRequiredService<IMultiTenantContextAccessor>()
            .MultiTenantContext = new MultiTenantContext<HeroTenantInfo>() { TenantInfo = tenant };

        // Then run the initialization in the new scope
        return await scope.ServiceProvider.GetRequiredService<ApplicationDbInitializer>()
            .InitializeAsync(ct);
    }

    private async Task<bool> InitializeTenantDbAsync(CancellationToken ct)
    {
        if ((await _tenantDbContext.Database.GetPendingMigrationsAsync(ct)).Any())
        {
            _logger.LogInformation("Applying Root Migrations.");
            await _tenantDbContext.Database.MigrateAsync(ct);

            await SeedRootTenantAsync(ct);

            return true;
        }

        return false;
    }

    private async Task SeedRootTenantAsync(CancellationToken ct)
    {
        if (await _tenantDbContext.TenantInfo.FindAsync(
            new object?[] { MultitenancyConstants.Root.Id }, ct) is null)
        {
            var rootTenant = new HeroTenantInfo(
                MultitenancyConstants.Root.Id,
                MultitenancyConstants.Root.Name,
                "",
                MultitenancyConstants.Root.EmailAddress);

            rootTenant.SetValidity(DateTime.UtcNow.AddYears(1));

            _tenantDbContext.TenantInfo.Add(rootTenant);

            await _tenantDbContext.SaveChangesAsync(ct);
        }
    }
}