using Heroplate.Api.Application.AppSettings;
using Heroplate.Api.Application.Common.Exceptions;
using Heroplate.Api.Application.Common.Persistence;
using Heroplate.Api.Domain.AppSettings;
using Heroplate.Api.Infrastructure.Multitenancy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;

namespace Heroplate.Api.Infrastructure.AppSettings;

public class AppSettingRepository : IAppSettingRepository
{
    private readonly IRepositoryWithEvents<AppSetting> _appSettingRepo;
    private readonly IConfiguration _config;
    private readonly HeroTenantInfo _currentTenant;
    private readonly IStringLocalizer _localizer;
    public AppSettingRepository(IRepositoryWithEvents<AppSetting> appSettingRepo, IConfiguration config, HeroTenantInfo currentTenant, IStringLocalizer<GetAppSettingsRequest> localizer) =>
        (_appSettingRepo, _config, _currentTenant, _localizer) = (appSettingRepo, config, currentTenant, localizer);

    public List<AppSettingDto> GetAll() =>
        DefaultAppSettings.All
            .Select(defaultSetting =>
                _config[$"{_currentTenant.Id}:{defaultSetting.Id}"] is { } configSetting && !string.IsNullOrWhiteSpace(configSetting)
                    ? defaultSetting with { Value = configSetting }
                    : defaultSetting)
            .ToList();

    public string Get(string id) =>
        DefaultAppSettings.All.FirstOrDefault(s => s.Id == id) is not { } defaultSetting
            ? throw new NotFoundException(_localizer["AppSetting '{0}' not found.", id])
        : _config[$"{_currentTenant.Id}:{id}"] is { } tenantValue && !string.IsNullOrWhiteSpace(tenantValue)
            ? tenantValue
        : _config[id] is { } globalValue && !string.IsNullOrWhiteSpace(globalValue)
            ? globalValue
        : defaultSetting.Value;

    public async Task UpdateAsync(string id, string? value, CancellationToken ct)
    {
        var defaultSetting = DefaultAppSettings.All.FirstOrDefault(s => s.Id == id)
            ?? throw new NotFoundException(_localizer["AppSetting '{0}' not found.", id]);

        var settingInDb = await _appSettingRepo.GetByIdAsync(id, ct);

        if (!string.IsNullOrWhiteSpace(value))
        {
            if (settingInDb is null)
            {
                settingInDb = new AppSetting(id, value);
                await _appSettingRepo.AddAsync(settingInDb, ct);
            }
            else if (settingInDb.Value != value)
            {
                settingInDb.Update(value);
                await _appSettingRepo.UpdateAsync(settingInDb, ct);
            }
        }
        else if (settingInDb is not null)
        {
            await _appSettingRepo.DeleteAsync(settingInDb, ct);
        }
    }
}