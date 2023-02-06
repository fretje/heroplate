namespace Heroplate.Admin.Application.AppSettings;

public partial class AppSettings
{
    [CascadingParameter]
    protected Task<AuthenticationState> AuthState { get; set; } = default!;

    protected ICollection<AppSettingDto>? AppSettingsList { get; set; }

    private KeyValueDto? _appSettingBackup;
    private string _searchString = "";
    private bool _canUpdate;

    protected override async Task OnInitializedAsync()
    {
        var state = await AuthState;
        _canUpdate = await Authorizer.HasPermissionAsync(state.User, Permissions.AppSettings.Update);

        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        AppSettingsList = null;
        AppSettingsList =
            (await ApiHelper.ExecuteCallGuardedAsync(
                AppSettingsClient.GetAllAsync, Snackbar))
            ?.ToList();
    }

    private void BackupAppSetting(object appSetting)
    {
        if (appSetting is AppSettingDto appSettingDto)
        {
            _appSettingBackup = new()
            {
                Key = appSettingDto.Id,
                Value = appSettingDto.Value,
            };
        }
    }

    private void ResetAppSettingToOriginalValue(object appSetting)
    {
        if (_appSettingBackup is not null
            && appSetting is AppSettingDto appSettingDto)
        {
            appSettingDto.Value = _appSettingBackup.Value;
        }
    }

    private void AppSettingHasBeenCommitted(object appSetting)
    {
        _appSettingBackup = null;

        if (appSetting is AppSettingDto appSettingDto)
        {
            _ = InvokeAsync(async () =>
            {
                var request = new UpdateAppSettingsRequest()
                {
                    AppSettings = new KeyValueDto[]
                    {
                        new() { Key = appSettingDto.Id, Value = appSettingDto.Value }
                    }
                };

                if (!await ApiHelper.ExecuteCallGuardedAsync(
                    () => AppSettingsClient.UpdateAsync(request),
                    Snackbar,
                    successMessage: L["Application Settings Saved"]))
                {
                    await LoadDataAsync();
                }
            });
        }
    }

    private bool Search(AppSettingDto appSetting) =>
        string.IsNullOrWhiteSpace(_searchString)
            || appSetting.Category.Contains(_searchString, StringComparison.OrdinalIgnoreCase)
            || appSetting.Name.Contains(_searchString, StringComparison.OrdinalIgnoreCase)
            || appSetting.Value.Contains(_searchString, StringComparison.OrdinalIgnoreCase);
}