using Heroplate.Api.Application.Identity.Roles;

namespace Heroplate.Api.Host.Controllers.Identity;

public class RolesController : VersionNeutralApiController
{
    private readonly IRoleService _roleService;
    public RolesController(IRoleService roleService) => _roleService = roleService;

    [HttpGet]
    [PermissionsAuthorize(Permissions.Roles.View)]
    [OpenApiOperation("Get a list of all roles.", "")]
    public Task<List<RoleDto>> GetListAsync(CancellationToken ct)
    {
        return _roleService.GetListAsync(ct);
    }

    [HttpGet("{id}")]
    [PermissionsAuthorize(Permissions.Roles.View)]
    [OpenApiOperation("Get role details.", "")]
    public Task<RoleDto> GetByIdAsync(string id)
    {
        return _roleService.GetByIdAsync(id);
    }

    [HttpGet("{id}/permissions")]
    [PermissionsAuthorize(Permissions.Roles.View, Permissions.RoleClaims.View, AllRequired = true)]
    [OpenApiOperation("Get role details with its permissions.", "")]
    public Task<RoleDto> GetByIdWithPermissionsAsync(string id, CancellationToken ct)
    {
        return _roleService.GetByIdWithPermissionsAsync(id, ct);
    }

    [HttpPut("{id}/permissions")]
    [PermissionsAuthorize(Permissions.RoleClaims.Update)]
    [OpenApiOperation("Update a role's permissions.", "")]
    public async Task<ActionResult<string>> UpdatePermissionsAsync(string id, UpdateRolePermissionsRequest req, CancellationToken ct)
    {
        return id != req.RoleId ? BadRequest() : Ok(await Mediator.Send(req, ct));
    }

    [HttpPost]
    [PermissionsAuthorize(Permissions.Roles.Create)]
    [OpenApiOperation("Create or update a role.", "")]
    [ApiConventionMethod(typeof(BaseApiConventions), nameof(BaseApiConventions.Search))]
    public Task<string> RegisterRoleAsync(CreateOrUpdateRoleRequest req, CancellationToken ct)
    {
        return Mediator.Send(req, ct);
    }

    [HttpDelete("{id}")]
    [PermissionsAuthorize(Permissions.Roles.Delete)]
    [OpenApiOperation("Delete a role.", "")]
    public Task<string> DeleteAsync(string id, CancellationToken ct)
    {
        return _roleService.DeleteAsync(id, ct);
    }
}