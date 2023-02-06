using Finbuckle.MultiTenant;
using Hangfire;
using Hangfire.Server;
using Heroplate.Api.Contracts.Multitenancy;
using Heroplate.Api.Infrastructure.Auth;
using Heroplate.Api.Infrastructure.Common;
using Heroplate.Api.Infrastructure.Multitenancy;
using Microsoft.Extensions.DependencyInjection;

namespace Heroplate.Api.Infrastructure.BackgroundJobs;

public class HeroJobActivator : JobActivator
{
    private readonly IServiceScopeFactory _scopeFactory;

    public HeroJobActivator(IServiceScopeFactory scopeFactory) =>
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));

    public override JobActivatorScope BeginScope(PerformContext context) =>
        new Scope(context, _scopeFactory.CreateScope());

    private class Scope : JobActivatorScope, IServiceProvider
    {
        private readonly PerformContext _context;
        private readonly IServiceScope _scope;

        public Scope(PerformContext context, IServiceScope scope)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _scope = scope ?? throw new ArgumentNullException(nameof(scope));

            SetTenantIdAndUserId();
        }

        private void SetTenantIdAndUserId()
        {
            var job = _context.BackgroundJob.Job;

            // Check if we're dealing with the send method which contains the tenant and userId as arguments
            // When recurring jobs start, we don't get a chance to set the parameters. This is a workaround.
            // If we can't get the parameters from there, we fall back to getting them from the actual parameters.
            if (!(job.Type == typeof(HangfireMediatorBridge)
                && job.Method.GetParameters() is { } parameters
                && parameters.Length == 6
                && job.Args[1] is string tenantId && job.Args[2] is string userId))
            {
                tenantId = _context.GetJobParameter<string>(MultitenancyConstants.TenantIdName);
                userId = _context.GetJobParameter<string>(QueryStringKeys.UserId);
            }

            if (!string.IsNullOrWhiteSpace(tenantId))
            {
                var tenantInfo = _scope.ServiceProvider
                    .GetRequiredService<TenantDbContext>()
                    .TenantInfo.Find(tenantId);

                if (tenantInfo is not null)
                {
                    _scope.ServiceProvider.GetRequiredService<IMultiTenantContextAccessor>()
                        .MultiTenantContext =
                            new MultiTenantContext<HeroTenantInfo> { TenantInfo = tenantInfo };
                }
            }

            if (!string.IsNullOrWhiteSpace(userId))
            {
                _scope.ServiceProvider.GetRequiredService<ICurrentUserInitializer>()
                    .SetCurrentUserId(userId);
            }
        }

        public override object Resolve(Type type) =>
            ActivatorUtilities.GetServiceOrCreateInstance(this, type);

        object? IServiceProvider.GetService(Type serviceType) =>
            serviceType == typeof(PerformContext)
                ? _context
                : _scope.ServiceProvider.GetService(serviceType);

        public override void DisposeScope() => _scope.Dispose();
    }
}