namespace Heroplate.Api.Application.Identity.Roles;

public interface IRoleService : ITransientService
{
    Task<List<RoleDto>> GetListAsync(CancellationToken ct);

    Task<int> GetCountAsync(CancellationToken ct);

    Task<bool> ExistsAsync(string roleName, string? excludeId);

    Task<RoleDto> GetByIdAsync(string id);

    Task<RoleDto> GetByIdWithPermissionsAsync(string roleId, CancellationToken ct);

    Task<string> CreateOrUpdateAsync(CreateOrUpdateRoleRequest req, CancellationToken ct);

    Task<string> UpdatePermissionsAsync(UpdateRolePermissionsRequest req, CancellationToken ct);

    Task<string> DeleteAsync(string id, CancellationToken ct);
}