using Hangfire;
using Hangfire.Console;
using Hangfire.Console.Extensions;
using Hangfire.SqlServer;
using HangfireBasicAuthenticationFilter;
using Heroplate.Api.Infrastructure.BackgroundJobs;
using Heroplate.Api.Infrastructure.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Serilog;

namespace Heroplate.Api.Infrastructure.BackgroundJobs;

internal static class Startup
{
    private static readonly ILogger _logger = Log.ForContext(typeof(Startup));

    internal static IServiceCollection AddBackgroundJobs(this IServiceCollection services, IConfiguration config)
    {
        services.AddHangfireServer(options => config.GetSection("HangfireSettings:Server").Bind(options));

        bool useConsole = string.Equals(config["HangfireSettings:UseConsole"], "true", StringComparison.OrdinalIgnoreCase);
        if (useConsole)
        {
            services.AddHangfireConsoleExtensions();
        }

        services.AddOptions<HangfireStorageSettings>()
            .BindConfiguration("HangfireSettings:Storage")
            .PostConfigure(storageSettings => _logger.Information($"Hangfire: Current Storage Provider: {storageSettings.StorageProvider}"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<JobActivator, HeroJobActivator>();

        services.AddHangfire((sp, hangfireConfig) =>
        {
            var storageSettings = sp.GetRequiredService<IOptions<HangfireStorageSettings>>().Value;
            hangfireConfig
                .UseDatabase(storageSettings.StorageProvider, storageSettings.ConnectionString, config)
                .UseFilter(new HeroJobFilter(sp))
                .UseFilter(new LogJobFilter())
                .UseSerializerSettings(new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All }); // necessary for MediatorHangfireBridge

            if (useConsole)
            {
                hangfireConfig.UseConsole();
            }
        });

        return services;
    }

    private static IGlobalConfiguration UseDatabase(this IGlobalConfiguration hangfireConfig, string dbProvider, string connectionString, IConfiguration config) =>
        dbProvider.ToLowerInvariant() switch
        {
            DbProviderKeys.SqlServer =>
                hangfireConfig.UseSqlServerStorage(connectionString, config.GetSection("HangfireSettings:Storage:Options").Get<SqlServerStorageOptions>()),
            _ => throw new Exception($"Hangfire Storage Provider {dbProvider} is not supported.")
        };

    internal static IApplicationBuilder UseHangfireDashboard(this IApplicationBuilder app, IConfiguration config)
    {
        var dashboardOptions = config.GetSection("HangfireSettings:Dashboard").Get<DashboardOptions>();

        if (dashboardOptions is null)
        {
            return app;
        }

        string? user = config.GetSection("HangfireSettings:Credentials:User")?.Value;
        string? pass = config.GetSection("HangfireSettings:Credentials:Password")?.Value;

        if (!string.IsNullOrWhiteSpace(user) && !string.IsNullOrWhiteSpace(pass))
        {
            dashboardOptions.Authorization = new[]
            {
               new HangfireCustomBasicAuthenticationFilter
               {
                    User = user,
                    Pass = pass
               }
            };
        }

        return app.UseHangfireDashboard(config["HangfireSettings:Route"], dashboardOptions);
    }
}