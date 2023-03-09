namespace Heroplate.Api.Application.AppSettings;

public class GetAppSettingsRequest : IRequest<List<AppSettingDto>>
{
}

public class GetAppSettingsRequestHandler : IRequestHandler<GetAppSettingsRequest, List<AppSettingDto>>
{
    private readonly IAppSettingRepository _appSettings;
    public GetAppSettingsRequestHandler(IAppSettingRepository appSettings) => _appSettings = appSettings;

    public Task<List<AppSettingDto>> Handle(GetAppSettingsRequest request, CancellationToken cancellationToken) =>
        Task.FromResult(_appSettings.GetAll());
}