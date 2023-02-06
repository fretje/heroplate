using Heroplate.Api.Application.AppSettings;

namespace Heroplate.Api.Host.Controllers.AppSettings;

public class AppSettingsController : VersionedApiController
{
    [HttpGet]
    [PermissionsAuthorize(Permissions.AppSettings.View)]
    [OpenApiOperation("Get list of application settings.", "")]
    public Task<List<AppSettingDto>> GetAllAsync(CancellationToken ct)
    {
        return Mediator.Send(new GetAppSettingsRequest(), ct);
    }

    [HttpPut]
    [PermissionsAuthorize(Permissions.AppSettings.Update)]
    [OpenApiOperation("Update the application settings.", "")]
    [ApiConventionMethod(typeof(BaseApiConventions), nameof(BaseApiConventions.BasePut))]
    public async Task<ActionResult> UpdateAsync(UpdateAppSettingsRequest req, CancellationToken ct)
    {
        await Mediator.Send(req, ct);
        return NoContent();
    }
}