using System.Reflection;
using System.Runtime.CompilerServices;
using Hellang.Middleware.ProblemDetails;
using Heroplate.Api.Application.Common.Interfaces;
using Heroplate.Api.Infrastructure;
using Heroplate.Api.Infrastructure.AppSettings;
using Heroplate.Api.Infrastructure.Auth;
using Heroplate.Api.Infrastructure.BackgroundJobs;
using Heroplate.Api.Infrastructure.Common;
using Heroplate.Api.Infrastructure.Common.Caching;
using Heroplate.Api.Infrastructure.Common.Cors;
using Heroplate.Api.Infrastructure.Common.FileStorage;
using Heroplate.Api.Infrastructure.Common.Localization;
using Heroplate.Api.Infrastructure.Common.Logging;
using Heroplate.Api.Infrastructure.Common.Mailing;
using Heroplate.Api.Infrastructure.Common.OpenApi;
using Heroplate.Api.Infrastructure.Common.ProblemDetails;
using Heroplate.Api.Infrastructure.Common.Validation;
using Heroplate.Api.Infrastructure.Multitenancy;
using Heroplate.Api.Infrastructure.Notifications;
using Heroplate.Api.Infrastructure.Persistence;
using Heroplate.Api.Infrastructure.Persistence.Initialization;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: InternalsVisibleTo("Api.Infrastructure.Tests")]

namespace Heroplate.Api.Infrastructure;

public static class Startup
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var assemblies = new[]
        {
            Assembly.GetExecutingAssembly(), // Infrastructure
            typeof(IApplicationMarker).Assembly // Application
        };

        return services
            .AddHeroProblemDetails()
            .AddApiVersioning()
            .AddAuth(config)
            .AddBackgroundJobs(config)
            .AddCaching(config)
            .AddCorsPolicy(config)
            .AddHealthCheck()
            .AddPOLocalization(config)
            .AddMailing(config)
            .AddMediatR(assemblies)
            .AddFluentValidation(assemblies)
            .AddMultitenancy()
            .AddNotifications(config)
            .AddOpenApiDocumentation(config)
            .AddPersistence()
            .AddRequestLogging(config)
            .AddRouting(options => options.LowercaseUrls = true)
            .AddServices()
            .AddResilientHttpClient();
    }

    private static IServiceCollection AddApiVersioning(this IServiceCollection services) =>
        services.AddApiVersioning(config =>
        {
            config.DefaultApiVersion = new ApiVersion(1, 0);
            config.AssumeDefaultVersionWhenUnspecified = true;
            config.ReportApiVersions = true;
        });

    private static IServiceCollection AddHealthCheck(this IServiceCollection services) =>
        services.AddHealthChecks().AddCheck<TenantHealthCheck>("Tenant").Services;

    public static async Task InitializeDatabasesAsync(this IServiceProvider services, CancellationToken ct = default)
    {
        // Create a new scope to retrieve scoped services
        using var scope = services.CreateScope();

        if (await scope.ServiceProvider.GetRequiredService<IDatabaseInitializer>()
            .InitializeDatabasesAsync(ct))
        {
            // Reload the app settings configuration when the database has been initialized (migrations applied)
            scope.ServiceProvider.GetRequiredService<IAppSettingsConfigurationReloader>().Reload();
        }
    }

    public static IApplicationBuilder UseInfrastructure(this IApplicationBuilder builder, IConfiguration config) =>
        builder
            .UseStaticFiles()
            .UseFileStorage()
            .UseOpenApiDocumentation(config)
            .UseHangfireDashboard(config)
            .UseRequestLogging()
            .UseProblemDetails()
            .UseRequestLocalization()
            .UseRouting()
            .UseCorsPolicy()
            .UseAuthentication()
            .UseCurrentUser()
            .UseMultiTenancy()
            .UseAuthorization();

    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapControllers().RequireAuthorization();
        builder.MapHealthCheck();
        builder.MapNotifications();
        return builder;
    }

    private static IEndpointConventionBuilder MapHealthCheck(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapHealthChecks("/api/health").RequireAuthorization();
}