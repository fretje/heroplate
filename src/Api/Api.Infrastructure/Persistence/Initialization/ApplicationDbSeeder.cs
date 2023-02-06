using Heroplate.Api.Contracts.Multitenancy;
using Heroplate.Api.Infrastructure.Identity;
using Heroplate.Api.Infrastructure.Multitenancy;
using Heroplate.Api.Infrastructure.Persistence.Context;
using Heroplate.Shared.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Heroplate.Api.Infrastructure.Persistence.Initialization;

internal class ApplicationDbSeeder
{
    private readonly HeroTenantInfo _currentTenant;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly CustomSeederRunner _seederRunner;
    private readonly ILogger<ApplicationDbSeeder> _logger;

    public ApplicationDbSeeder(HeroTenantInfo currentTenant, RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager, CustomSeederRunner seederRunner, ILogger<ApplicationDbSeeder> logger)
    {
        _currentTenant = currentTenant;
        _roleManager = roleManager;
        _userManager = userManager;
        _seederRunner = seederRunner;
        _logger = logger;
    }

    public async Task SeedDatabaseAsync(ApplicationDbContext dbContext, CancellationToken ct)
    {
        await SeedRolesAsync(dbContext, ct);
        await SeedAdminUserAsync();
        await _seederRunner.RunSeedersAsync(ct);
    }

    private async Task SeedRolesAsync(ApplicationDbContext dbContext, CancellationToken ct)
    {
        foreach (string roleName in ApplicationRoles.DefaultRoles)
        {
            if (await _roleManager.Roles.SingleOrDefaultAsync(r => r.Name == roleName, ct)
                is not ApplicationRole role)
            {
                // Create the role
                _logger.LogDebug("Seeding {Role} Role for '{TenantId}' Tenant.", roleName, _currentTenant.Id);
                role = new ApplicationRole(roleName, $"{roleName} Role for {_currentTenant.Id} Tenant");
                await _roleManager.CreateAsync(role);
            }

            // Assign permissions
            if (roleName == ApplicationRoles.Basic)
            {
                await AssignPermissionsToRoleAsync(dbContext, PermissionProvider.Basic, role, ct);
            }
            else if (roleName == ApplicationRoles.Admin)
            {
                await AssignPermissionsToRoleAsync(dbContext, PermissionProvider.Admin, role, ct);

                if (_currentTenant.Id == MultitenancyConstants.Root.Id)
                {
                    await AssignPermissionsToRoleAsync(dbContext, PermissionProvider.Root, role, ct);
                }
            }
        }
    }

    private async Task AssignPermissionsToRoleAsync(ApplicationDbContext dbContext, string[] permissions, ApplicationRole role, CancellationToken ct)
    {
        var currentClaims = await _roleManager.GetClaimsAsync(role);
        foreach (string permission in permissions)
        {
            if (!currentClaims.Any(c => c.Type == CustomClaimTypes.Permission && c.Value == permission))
            {
                _logger.LogDebug("Seeding {Role} Permission '{Permission}' for '{TenantId}' Tenant.", role.Name, permission, _currentTenant.Id);
                dbContext.RoleClaims.Add(new ApplicationRoleClaim
                {
                    RoleId = role.Id,
                    ClaimType = CustomClaimTypes.Permission,
                    ClaimValue = permission,
                    CreatedBy = "ApplicationDbSeeder"
                });
                await dbContext.SaveChangesAsync(ct);
            }
        }
    }

    private async Task SeedAdminUserAsync()
    {
        if (string.IsNullOrWhiteSpace(_currentTenant.Id) || string.IsNullOrWhiteSpace(_currentTenant.AdminEmail))
        {
            return;
        }

        if (await _userManager.Users.FirstOrDefaultAsync(u => u.Email == _currentTenant.AdminEmail)
            is not ApplicationUser adminUser)
        {
            string adminUserName = $"{_currentTenant.Id.Trim()}.{ApplicationRoles.Admin}".ToLowerInvariant();
            adminUser = new ApplicationUser
            {
                FirstName = _currentTenant.Id.Trim().ToLowerInvariant(),
                LastName = ApplicationRoles.Admin,
                Email = _currentTenant.AdminEmail,
                UserName = adminUserName,
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                NormalizedEmail = _currentTenant.AdminEmail?.ToUpperInvariant(),
                NormalizedUserName = adminUserName.ToUpperInvariant(),
                IsActive = true
            };
            if (_currentTenant.Id == MultitenancyConstants.Root.Id)
            {
                adminUser.Id = MultitenancyConstants.Root.AdminUserId;
            }

            _logger.LogDebug("Seeding Default Admin User for '{TenantId}' Tenant.", _currentTenant.Id);
            var password = new PasswordHasher<ApplicationUser>();
            adminUser.PasswordHash = password.HashPassword(adminUser, MultitenancyConstants.DefaultPassword);
            await _userManager.CreateAsync(adminUser);
        }

        // Assign role to user
        if (!await _userManager.IsInRoleAsync(adminUser, ApplicationRoles.Admin))
        {
            _logger.LogDebug("Assigning Admin Role to Admin User for '{TenantId}' Tenant.", _currentTenant.Id);
            await _userManager.AddToRoleAsync(adminUser, ApplicationRoles.Admin);
        }
    }
}