using Heroplate.Api.Application.Identity.Users;
using Heroplate.Api.Application.Identity.Users.Password;
using Heroplate.Api.Infrastructure.Common.OpenApi;

namespace Heroplate.Api.Host.Controllers.Identity;

public class UsersController : VersionNeutralApiController
{
    private readonly IUserService _userService;
    public UsersController(IUserService userService) => _userService = userService;

    [HttpGet]
    [PermissionsAuthorize(Permissions.Users.View)]
    [OpenApiOperation("Get list of all users.", "")]
    public Task<List<UserDetailsDto>> GetListAsync(CancellationToken ct)
    {
        return _userService.GetListAsync(ct);
    }

    [HttpGet("{id}")]
    [PermissionsAuthorize(Permissions.Users.View)]
    [OpenApiOperation("Get a user's details.", "")]
    public Task<UserDetailsDto> GetByIdAsync(string id, CancellationToken ct)
    {
        return _userService.GetAsync(id, ct);
    }

    [HttpGet("{id}/roles")]
    [PermissionsAuthorize(Permissions.UserRoles.View)]
    [OpenApiOperation("Get a user's roles.", "")]
    public Task<List<UserRoleDto>> GetRolesAsync(string id, CancellationToken ct)
    {
        return _userService.GetRolesAsync(id, ct);
    }

    [HttpPost("{id}/roles")]
    [ApiConventionMethod(typeof(BaseApiConventions), nameof(BaseApiConventions.Search))]
    [PermissionsAuthorize(Permissions.UserRoles.Update)]
    [OpenApiOperation("Update a user's assigned roles.", "")]
    public Task<string> AssignRolesAsync(string id, UserRolesRequest req, CancellationToken ct)
    {
        return _userService.AssignRolesAsync(id, req, ct);
    }

    [HttpPost("{id}/toggle-status")]
    [PermissionsAuthorize(Permissions.Users.Update)]
    [OpenApiOperation("Toggle a user's active status.", "")]
    [ApiConventionMethod(typeof(BaseApiConventions), nameof(BaseApiConventions.Search))]
    public async Task<ActionResult> ToggleStatusAsync(string id, ToggleUserStatusRequest req, CancellationToken ct)
    {
        if (id != req.UserId)
        {
            return BadRequest();
        }

        await _userService.ToggleStatusAsync(req, ct);
        return Ok();
    }

    [HttpPost]
    [PermissionsAuthorize(Permissions.Users.Create)]
    [OpenApiOperation("Creates a new user.", "")]
    public Task<string> CreateAsync(CreateUserRequest request, CancellationToken ct)
    {
        // TODO: check if registering anonymous users is actually allowed (should probably be an appsetting)
        // and return UnAuthorized when it isn't
        // Also: add other protection to prevent automatic posting (captcha?)
        return _userService.CreateAsync(request, GetOriginFromRequest(), ct);
    }

    [HttpPost("self-register")]
    [TenantIdHeader]
    [AllowAnonymous]
    [OpenApiOperation("Anonymous user creates a user.", "")]
    [ApiConventionMethod(typeof(BaseApiConventions), nameof(BaseApiConventions.Search))]
    public Task<string> SelfRegisterAsync(CreateUserRequest request, CancellationToken ct)
    {
        // TODO: check if registering anonymous users is actually allowed (should probably be an appsetting)
        // and return UnAuthorized when it isn't
        // Also: add other protection to prevent automatic posting (captcha?)
        return _userService.CreateAsync(request, GetOriginFromRequest(), ct);
    }

    [HttpGet("confirm-email")]
    [AllowAnonymous]
    [OpenApiOperation("Confirm email address for a user.", "")]
    [ApiConventionMethod(typeof(BaseApiConventions), nameof(BaseApiConventions.BaseGet))]
    public Task<string> ConfirmEmailAsync([FromQuery] string userId, [FromQuery] string code, CancellationToken ct)
    {
        return _userService.ConfirmEmailAsync(userId, code, ct);
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [TenantIdHeader]
    [OpenApiOperation("Request a password reset email for a user.", "")]
    [ApiConventionMethod(typeof(BaseApiConventions), nameof(BaseApiConventions.Search))]
    public Task<string> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        return _userService.ForgotPasswordAsync(request, GetOriginFromRequest());
    }

    [HttpPost("reset-password")]
    [OpenApiOperation("Reset a user's password.", "")]
    [ApiConventionMethod(typeof(BaseApiConventions), nameof(BaseApiConventions.Search))]
    public Task<string> ResetPasswordAsync(ResetPasswordRequest request)
    {
        return _userService.ResetPasswordAsync(request);
    }

    private string GetOriginFromRequest() => $"{Request.Scheme}://{Request.Host.Value}{Request.PathBase.Value}";
}