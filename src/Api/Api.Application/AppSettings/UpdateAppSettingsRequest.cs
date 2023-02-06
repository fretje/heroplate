namespace Heroplate.Api.Application.AppSettings;

public class UpdateAppSettingsRequest : IRequest
{
    public List<KeyValueDto> AppSettings { get; set; } = default!;
}

public class UpdateAppSettingsRequestValidator : AbstractValidator<UpdateAppSettingsRequest>
{
    public UpdateAppSettingsRequestValidator() =>
        RuleFor(p => p.AppSettings)
            .NotEmpty();
}

public class UpdateAppSettingsRequestHandler : AsyncRequestHandler<UpdateAppSettingsRequest>
{
    private readonly IAppSettingRepository _appSettings;
    public UpdateAppSettingsRequestHandler(IAppSettingRepository appSettings) => _appSettings = appSettings;

    protected override async Task Handle(UpdateAppSettingsRequest req, CancellationToken ct)
    {
        foreach (var setting in req.AppSettings)
        {
            await _appSettings.UpdateAsync(setting.Key, setting.Value, ct);
        }
    }
}