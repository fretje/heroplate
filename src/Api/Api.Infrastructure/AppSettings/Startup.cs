using Heroplate.Api.Application.AppSettings;
using Heroplate.Api.Infrastructure.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Heroplate.Api.Infrastructure.AppSettings;

public static class Startup
{
    public static void AddAppSettingsConfiguration(this WebApplicationBuilder builder)
    {
        builder.Services.AddTransient<IAppSettingRepository, AppSettingRepository>();

        // we have to get the database settings this way to not have to require a serviceprovider
        var dbSettings = builder.Configuration.GetRequiredSection(nameof(DatabaseSettings)).Get<DatabaseSettings>()
            ?? throw new InvalidOperationException("No DatabaseSettings configured.");

        var reloader = new AppSettingsConfigurationReloader();
        builder.Services.AddSingleton<IAppSettingsConfigurationReloader>(_ => reloader);

        builder.Configuration.AddAppSettingsConfiguration(
            b => b.UseDatabase(dbSettings.DBProvider, dbSettings.ConnectionString),
            reloader);
    }

    internal static IConfigurationBuilder AddAppSettingsConfiguration(this IConfigurationBuilder builder, Action<DbContextOptionsBuilder> optionsAction, AppSettingsConfigurationReloader? reloader) =>
        builder.Add(new AppSettingsConfigurationSource(optionsAction, reloader));
}