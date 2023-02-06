using System.Security.Claims;

namespace Heroplate.Shared.Authorization;

public static class ClaimsIdentityExtensions
{
    public static void AddPermissions(this ClaimsIdentity identity, string[] permissions) =>
        identity.AddClaims(
            permissions.Select(
                permission => new Claim(CustomClaimTypes.Permission, permission)));
}