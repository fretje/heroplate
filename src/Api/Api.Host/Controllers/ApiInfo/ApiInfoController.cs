using Heroplate.Api.Infrastructure.Common;

namespace Heroplate.Api.Host.Controllers.ApiInfo;

[Route("/api")]
[AllowAnonymous]
public class ApiInfoController : BaseApiController
{
    [HttpGet]
    public ApiInfoDto GetInfo()
    {
        return new(
            product: "Heroplate Api",
            version: VersionInfo.Version.ToString(),
            gitSha: VersionInfo.GitSha);
    }
}