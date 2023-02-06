using Heroplate.Api.Application.Common.Exceptions;
using Heroplate.Api.Application.Identity.Users;
using Heroplate.Api.Contracts.Multitenancy;
using Heroplate.Api.Domain.Identity;
using Heroplate.Shared.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Heroplate.Api.Infrastructure.Identity;

internal partial class UserService
{
    public async Task<List<UserRoleDto>> GetRolesAsync(string userId, CancellationToken ct)
    {
        var userRoles = new List<UserRoleDto>();

        var user = await _userManager.FindByIdAsync(userId);
        if (user is not null)
        {
            var roles = await _roleManager.Roles.AsNoTracking().ToListAsync(ct);
            foreach (var role in roles.Where(r => r.Name != null))
            {
                userRoles.Add(new UserRoleDto
                {
                    RoleId = role.Id,
                    RoleName = role.Name,
                    Description = role.Description,
                    Enabled = await _userManager.IsInRoleAsync(user, role.Name!)
                });
            }
        }

        return userRoles;
    }

    public async Task<string> AssignRolesAsync(string userId, UserRolesRequest req, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(req, nameof(req));

        var user = await _userManager.Users.Where(u => u.Id == userId).FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(_t["User {0} not found.", userId]);

        // Check if the user is an admin for which the admin role is getting disabled
        if (await _userManager.IsInRoleAsync(user, ApplicationRoles.Admin)
            && req.UserRoles.Any(a => !a.Enabled && a.RoleName == ApplicationRoles.Admin))
        {
            // Get count of users in Admin Role
            int adminCount = (await _userManager.GetUsersInRoleAsync(ApplicationRoles.Admin)).Count;

            // Check if user is not Root Tenant Admin
            // Edge Case : there are chances for other tenants to have users with the same email as that of Root Tenant Admin. Probably can add a check while User Registration
            if (user.Email == MultitenancyConstants.Root.EmailAddress)
            {
                if (_currentTenant.Id == MultitenancyConstants.Root.Id)
                {
                    throw new ConflictException(_t["Cannot Remove Admin Role From Root Tenant Admin."]);
                }
            }
            else if (adminCount <= 2)
            {
                throw new ConflictException(_t["Tenant should have at least 2 Admins."]);
            }
        }

        foreach (var userRole in req.UserRoles.Where(r => !string.IsNullOrWhiteSpace(r.RoleName)))
        {
            // Check if Role Exists
            if (await _roleManager.FindByNameAsync(userRole.RoleName!) is not null)
            {
                if (userRole.Enabled)
                {
                    if (!await _userManager.IsInRoleAsync(user, userRole.RoleName!))
                    {
                        await _userManager.AddToRoleAsync(user, userRole.RoleName!);
                    }
                }
                else
                {
                    await _userManager.RemoveFromRoleAsync(user, userRole.RoleName!);
                }
            }
        }

        await _events.PublishAsync(new ApplicationUserUpdatedEvent(user.Id, user.ObjectId, true), ct);

        return _t["User Roles Updated Successfully."];
    }
}