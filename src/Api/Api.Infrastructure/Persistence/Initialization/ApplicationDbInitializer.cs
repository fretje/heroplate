using Finbuckle.MultiTenant;
using Heroplate.Api.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Heroplate.Api.Infrastructure.Persistence.Initialization;

internal class ApplicationDbInitializer
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ITenantInfo _currentTenant;
    private readonly ApplicationDbSeeder _dbSeeder;
    private readonly ILogger<ApplicationDbInitializer> _logger;

    public ApplicationDbInitializer(ApplicationDbContext dbContext, ITenantInfo currentTenant, ApplicationDbSeeder dbSeeder, ILogger<ApplicationDbInitializer> logger)
    {
        _dbContext = dbContext;
        _currentTenant = currentTenant;
        _dbSeeder = dbSeeder;
        _logger = logger;
    }

    public async Task<bool> InitializeAsync(CancellationToken ct)
    {
        if ((await _dbContext.Database.GetAppliedMigrationsAsync(ct)).Any())
        {
            if ((await _dbContext.Database.GetPendingMigrationsAsync(ct)).Any())
            {
                _logger.LogInformation("Applying Migrations for '{TenantId}' tenant.", _currentTenant.Id);
                await _dbContext.Database.MigrateAsync(ct);
            }

            if (await _dbContext.Database.CanConnectAsync(ct))
            {
                _logger.LogInformation("Connection to {TenantId}'s Database Succeeded.", _currentTenant.Id);

                await _dbSeeder.SeedDatabaseAsync(_dbContext, ct);
            }

            return true;
        }

        return false;
    }
}