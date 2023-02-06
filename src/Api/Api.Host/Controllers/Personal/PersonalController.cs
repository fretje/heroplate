using System.Security.Claims;
using Heroplate.Api.Application.Common.Auditing;
using Heroplate.Api.Application.Identity.Users;

namespace Heroplate.Api.Host.Controllers.Personal;

public class PersonalController : VersionNeutralApiController
{
    private readonly IUserService _userService;

    public PersonalController(IUserService userService) => _userService = userService;

    [HttpGet("profile")]
    [OpenApiOperation("Get profile details of currently logged in user.", "")]
    public async Task<ActionResult<UserDetailsDto>> GetProfileAsync(CancellationToken ct)
    {
        return User.GetUserId() is not { } userId || string.IsNullOrEmpty(userId)
            ? Unauthorized()
            : Ok(await _userService.GetAsync(userId, ct));
    }

    [HttpPut("profile")]
    [OpenApiOperation("Update profile details of currently logged in user.", "")]
    public async Task<ActionResult> UpdateProfileAsync(UpdateUserRequest req, CancellationToken ct)
    {
        if (User.GetUserId() is not { } userId || string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        if (req.Id != userId)
        {
            return BadRequest();
        }

        await Mediator.Send(req, ct);
        return Ok();
    }

    [HttpGet("permissions")]
    [OpenApiOperation("Get permissions of currently logged in user.", "")]
    public async Task<ActionResult<string[]>> GetPermissionsAsync(CancellationToken ct)
    {
        return User.GetUserId() is not { } userId || string.IsNullOrEmpty(userId)
            ? Unauthorized()
            : Ok(await _userService.GetPermissionsAsync(userId, ct));
    }

    [HttpGet("logs")]
    [OpenApiOperation("Get audit logs of currently logged in user.", "")]
    public Task<List<AuditDto>> GetLogsAsync(CancellationToken ct)
    {
        return Mediator.Send(new GetMyAuditLogsRequest(), ct);
    }
}