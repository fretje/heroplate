using Heroplate.Api.Application.AppSettings;
using Heroplate.Api.Domain.AppSettings;
using Heroplate.Api.Infrastructure.Multitenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace Heroplate.Api.Infrastructure.AppSettings;

internal sealed class AppSettingsConfigurationProvider : ConfigurationProvider, IDisposable
{
    private readonly AppSettingsConfigurationSource _source;
    private readonly IDisposable? _changeTokenRegistration;

    public AppSettingsConfigurationProvider(AppSettingsConfigurationSource source)
    {
        _source = source;
        if (_source.Reloader is not null)
        {
            _changeTokenRegistration = ChangeToken.OnChange(
                _source.Reloader.GetReloadToken,
                Load);
        }
    }

    public override void Load()
    {
        // Connect to TenantDb
        var tenantBuilder = new DbContextOptionsBuilder<TenantDbContext>();
        _source.OptionsAction(tenantBuilder);
        using var tenantDb = new TenantDbContext(tenantBuilder.Options);
        if (!tenantDb.Database.CanConnect() || tenantDb.Database.GetPendingMigrations().Any())
        {
            return;
        }

        string[] tenantIds = tenantDb.TenantInfo.Select(t => t.Id).ToArray();

        // Connect to SettingsDb
        var settingsBuilder = new DbContextOptionsBuilder<AppSettingsDbContext>();
        _source.OptionsAction(settingsBuilder);
        using var settingsDb = new AppSettingsDbContext(settingsBuilder.Options);
        if (!settingsDb.Database.CanConnect())
        {
            return;
        }

        var settingsInDb = settingsDb.AppSetting.ToList();

        Data = new Dictionary<string, string?>(
            tenantIds.SelectMany(tenantId =>
                DefaultAppSettings.All.Select(defaultSetting =>
                    settingsInDb.Find(s =>
                        s.TenantId.Equals(tenantId, StringComparison.OrdinalIgnoreCase) &&
                        s.Id.Equals(defaultSetting.Id, StringComparison.OrdinalIgnoreCase))
                    is AppSetting settingInDb
                        ? KeyValuePair.Create($"{tenantId}:{settingInDb.Id}", (string?)settingInDb.Value)
                        : KeyValuePair.Create($"{tenantId}:{defaultSetting.Id}", (string?)defaultSetting.Value))),
            StringComparer.OrdinalIgnoreCase);
    }

    public void Dispose() => _changeTokenRegistration?.Dispose();
}