using Heroplate.Api.Application.Common.Exceptions;
using Heroplate.Shared.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Heroplate.Api.Infrastructure.Identity;

internal partial class UserService
{
    public async Task<string[]> GetPermissionsAsync(string userId, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new NotFoundException(_t["User {0} not found.", userId]);

        var userRoles = await _userManager.GetRolesAsync(user);
        var permissions = new List<string>();

        foreach (var role in await _roleManager.Roles
            .Where(r => r.Name != null && userRoles.Contains(r.Name))
            .ToListAsync(ct))
        {
            permissions.AddRange(await _db.RoleClaims
                .Where(rc => rc.RoleId == role.Id && rc.ClaimType == CustomClaimTypes.Permission && !string.IsNullOrWhiteSpace(rc.ClaimValue))
                .Select(rc => rc.ClaimValue!)
                .ToArrayAsync(ct));
        }

        return permissions.Distinct().ToArray();
    }
}