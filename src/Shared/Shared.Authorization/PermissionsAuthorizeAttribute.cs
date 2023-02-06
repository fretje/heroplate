using Microsoft.AspNetCore.Authorization;

namespace Heroplate.Shared.Authorization;

public sealed class PermissionsAuthorizeAttribute : AuthorizeAttribute
{
    public PermissionsAuthorizeAttribute(params string[] permissions) =>
        Policy = PermissionsAuthorizationPolicyName.For(permissions, false);

    public string[] Permissions =>
        PermissionsAuthorizationPolicyName.PermissionsFrom(Policy);

    public bool AllRequired
    {
        get => PermissionsAuthorizationPolicyName.AllRequiredFrom(Policy);
        set => Policy = PermissionsAuthorizationPolicyName.For(Permissions, value);
    }
}