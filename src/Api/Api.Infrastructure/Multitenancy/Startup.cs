using Heroplate.Api.Application.Multitenancy;
using Heroplate.Api.Contracts.Multitenancy;
using Heroplate.Api.Infrastructure.Persistence;
using Heroplate.Shared.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Heroplate.Api.Infrastructure.Multitenancy;

internal static class Startup
{
    internal static IServiceCollection AddMultitenancy(this IServiceCollection services)
    {
        return services
            .AddDbContext<TenantDbContext>((p, m) =>
            {
                // TODO: We should probably add specific dbprovider/connectionstring setting for the tenantDb with a fallback to the main databasesettings
                var databaseSettings = p.GetRequiredService<IOptions<DatabaseSettings>>().Value;
                m.UseDatabase(databaseSettings.DBProvider, databaseSettings.ConnectionString);
            })
            .AddMultiTenant<HeroTenantInfo>()
                .WithClaimStrategy(CustomClaimTypes.Tenant)
                .WithHeaderStrategy(MultitenancyConstants.TenantIdName)
                .WithQueryStringStrategy(MultitenancyConstants.TenantIdName)
                .WithEFCoreStore<TenantDbContext, HeroTenantInfo>()
                .Services
            .AddScoped<ITenantService, TenantService>()
            .AddScoped<ICurrentTenantService, CurrentTenantService>();
    }

    internal static IApplicationBuilder UseMultiTenancy(this IApplicationBuilder app) =>
        app.UseMultiTenant();

    private static FinbuckleMultiTenantBuilder<HeroTenantInfo> WithQueryStringStrategy(this FinbuckleMultiTenantBuilder<HeroTenantInfo> builder, string queryStringKey) =>
        builder.WithDelegateStrategy(context =>
        {
            if (context is not HttpContext httpContext)
            {
                return Task.FromResult((string?)null);
            }

            httpContext.Request.Query.TryGetValue(queryStringKey, out var tenantIdParam);

            return Task.FromResult((string?)tenantIdParam.ToString());
        });
}