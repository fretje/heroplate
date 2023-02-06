using Finbuckle.MultiTenant;
using Heroplate.Api.Application.Common.Events;
using Heroplate.Api.Application.Common.Exceptions;
using Heroplate.Api.Application.Common.Interfaces;
using Heroplate.Api.Application.Identity.Roles;
using Heroplate.Api.Contracts.Multitenancy;
using Heroplate.Api.Domain.Identity;
using Heroplate.Api.Infrastructure.Persistence.Context;
using Heroplate.Shared.Authorization;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace Heroplate.Api.Infrastructure.Identity;

internal class RoleService : IRoleService
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _db;
    private readonly IStringLocalizer _t;
    private readonly ICurrentUser _currentUser;
    private readonly ITenantInfo _currentTenant;
    private readonly IEventPublisher _events;

    public RoleService(
        RoleManager<ApplicationRole> roleManager,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext db,
        IStringLocalizer<RoleService> localizer,
        ICurrentUser currentUser,
        ITenantInfo currentTenant,
        IEventPublisher events)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _db = db;
        _t = localizer;
        _currentUser = currentUser;
        _currentTenant = currentTenant;
        _events = events;
    }

    public async Task<List<RoleDto>> GetListAsync(CancellationToken ct) =>
        (await _roleManager.Roles.ToListAsync(ct))
            .Adapt<List<RoleDto>>();

    public Task<int> GetCountAsync(CancellationToken ct) =>
        _roleManager.Roles.CountAsync(ct);

    public async Task<bool> ExistsAsync(string roleName, string? excludeId) =>
        await _roleManager.FindByNameAsync(roleName)
            is ApplicationRole existingRole
            && existingRole.Id != excludeId;

    public async Task<RoleDto> GetByIdAsync(string id) =>
        await _db.Roles.SingleOrDefaultAsync(x => x.Id == id) is { } role
            ? role.Adapt<RoleDto>()
            : throw new NotFoundException(_t["Role with Id {0} not found.", id]);

    public async Task<RoleDto> GetByIdWithPermissionsAsync(string roleId, CancellationToken ct)
    {
        var role = await GetByIdAsync(roleId);

        role.Permissions = await _db.RoleClaims
            .Where(c => c.RoleId == roleId && c.ClaimType == CustomClaimTypes.Permission && !string.IsNullOrWhiteSpace(c.ClaimValue))
            .Select(c => c.ClaimValue!)
            .ToListAsync(ct);

        return role;
    }

    public async Task<string> CreateOrUpdateAsync(CreateOrUpdateRoleRequest req, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(req.Id))
        {
            // Create a new role.
            var role = new ApplicationRole(req.Name, req.Description);
            var result = await _roleManager.CreateAsync(role);

            if (!result.Succeeded)
            {
                throw new InternalServerException(_t["Register role failed"], result.GetErrors(_t));
            }

            await _events.PublishAsync(new ApplicationRoleCreatedEvent(role.Id, role.Name!), ct);

            return _t["Role {0} created.", req.Name];
        }
        else
        {
            // Update an existing role.
            var role = await _roleManager.FindByIdAsync(req.Id)
                ?? throw new NotFoundException(_t["Role with Id {0} not found.", req.Id]);

            if (ApplicationRoles.IsDefault(role.Name))
            {
                throw new ConflictException(_t["Not allowed to modify {0} Role.", role.Name!]);
            }

            role.Name = req.Name;
            role.NormalizedName = req.Name.ToUpperInvariant();
            role.Description = req.Description;
            var result = await _roleManager.UpdateAsync(role);

            if (!result.Succeeded)
            {
                throw new InternalServerException(_t["Update role failed"], result.GetErrors(_t));
            }

            await _events.PublishAsync(new ApplicationRoleUpdatedEvent(role.Id, role.Name), ct);

            return _t["Role {0} Updated.", role.Name];
        }
    }

    public async Task<string> UpdatePermissionsAsync(UpdateRolePermissionsRequest req, CancellationToken ct)
    {
        var role = await _roleManager.FindByIdAsync(req.RoleId)
            ?? throw new NotFoundException(_t["Role with Id {0} not found.", req.RoleId]);
        if (role.Name == ApplicationRoles.Admin)
        {
            throw new ConflictException(_t["Not allowed to modify Permissions for this Role."]);
        }

        // Remove Root Permissions if this is not the Root Tenant.
        if (_currentTenant.Id != MultitenancyConstants.Root.Id)
        {
            req.Permissions.RemoveAll(reqPermission =>
                PermissionProvider.Root.Any(rootPermission => rootPermission == reqPermission));
        }

        var currentClaims = await _roleManager.GetClaimsAsync(role);

        // Remove permissions that were previously selected
        foreach (var claim in currentClaims.Where(c => !req.Permissions.Any(p => p == c.Value)))
        {
            var removeResult = await _roleManager.RemoveClaimAsync(role, claim);
            if (!removeResult.Succeeded)
            {
                throw new InternalServerException(_t["Update permissions failed."], removeResult.GetErrors(_t));
            }
        }

        // Add all permissions that were not previously selected
        foreach (string permission in req.Permissions.Where(c => !currentClaims.Any(p => p.Value == c)))
        {
            if (!string.IsNullOrEmpty(permission))
            {
                _db.RoleClaims.Add(new ApplicationRoleClaim
                {
                    RoleId = role.Id,
                    ClaimType = CustomClaimTypes.Permission,
                    ClaimValue = permission,
                    CreatedBy = _currentUser.GetUserId().ToString()
                });
                await _db.SaveChangesAsync(ct);
            }
        }

        await _events.PublishAsync(new ApplicationRoleUpdatedEvent(role.Id, role.Name!, true), ct);

        return _t["Permissions Updated."];
    }

    public async Task<string> DeleteAsync(string id, CancellationToken ct)
    {
        var role = await _roleManager.FindByIdAsync(id)
            ?? throw new NotFoundException(_t["Role with Id {0} not found.", id]);

        if (ApplicationRoles.IsDefault(role.Name))
        {
            throw new ConflictException(_t["Not allowed to delete {0} Role.", role.Name!]);
        }

        if ((await _userManager.GetUsersInRoleAsync(role.Name!)).Count > 0)
        {
            throw new ConflictException(_t["Not allowed to delete {0} Role as it is being used.", role.Name!]);
        }

        await _roleManager.DeleteAsync(role);

        await _events.PublishAsync(new ApplicationRoleDeletedEvent(role.Id, role.Name!), ct);

        return _t["Role {0} Deleted.", role.Name!];
    }
}