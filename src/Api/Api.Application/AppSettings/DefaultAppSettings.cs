using System.Collections.ObjectModel;

namespace Heroplate.Api.Application.AppSettings;

public static class DefaultAppSettings
{
    private static readonly ReadOnlyCollection<AppSettingDto> _settings = new List<AppSettingDto>()
        {
            new(AppSettingIds.BackendApi.ApiUrl, ""),
            new(AppSettingIds.Exact.AuthorizationCallbackUrl, ""),
        }.AsReadOnly();

    public static IReadOnlyCollection<AppSettingDto> All => _settings;
}