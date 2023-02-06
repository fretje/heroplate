using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Heroplate.Api.Infrastructure.Multitenancy;

public class TenantHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken ct = default)
    {
        // Descoped
        var check = new HealthCheckResult(HealthStatus.Healthy);
        return Task.FromResult(check);
    }
}