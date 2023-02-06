namespace Heroplate.Api.Application.AppSettings;

public class GetAppSettingsRequest : IRequest<List<AppSettingDto>>
{
}

public class GetAppSettingsRequestHandler : RequestHandler<GetAppSettingsRequest, List<AppSettingDto>>
{
    private readonly IAppSettingRepository _appSettings;
    public GetAppSettingsRequestHandler(IAppSettingRepository appSettings) => _appSettings = appSettings;

    protected override List<AppSettingDto> Handle(GetAppSettingsRequest request) =>
        _appSettings.GetAll();
}