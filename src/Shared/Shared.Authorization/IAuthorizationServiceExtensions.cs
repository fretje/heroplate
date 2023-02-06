using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Heroplate.Shared.Authorization;

public static class IAuthorizationServiceExtensions
{
    public static Task<bool> HasPermissionAsync(this IAuthorizationService service, ClaimsPrincipal user, string permission) =>
        service.HasPermissionsAsync(user, new[] { permission });

    public static Task<bool> HasAnyPermissionAsync(this IAuthorizationService service, ClaimsPrincipal user, params string[] permissions) =>
        service.HasPermissionsAsync(user, permissions);

    public static Task<bool> HasAllPermissionsAsync(this IAuthorizationService service, ClaimsPrincipal user, params string[] permissions) =>
        service.HasPermissionsAsync(user, permissions, true);

    private static async Task<bool> HasPermissionsAsync(this IAuthorizationService service, ClaimsPrincipal user, string[] permissions, bool allRequired = false) =>
        (await service.AuthorizeAsync(user, PermissionsAuthorizationPolicyName.For(permissions, allRequired))).Succeeded;
}