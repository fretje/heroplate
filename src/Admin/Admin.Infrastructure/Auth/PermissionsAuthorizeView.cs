using Heroplate.Shared.Authorization;
using Microsoft.AspNetCore.Components.Authorization;

namespace Heroplate.Admin.Infrastructure.Auth;

public class PermissionsAuthorizeView : AuthorizeView
{
    [Parameter]
    public string? Permission { get; set; }
    [Parameter]
    public string[]? Permissions { get; set; }
    [Parameter]
    public bool AllRequired { get; set; }

    protected override void OnInitialized()
    {
        string[] permissions = string.IsNullOrEmpty(Permission)
            ? Permissions ?? throw new InvalidOperationException("Must provide at least one permission!")
            : Permissions is null
                ? new[] { Permission }
                : Permissions.Concat(new[] { Permission }).ToArray();

        Policy = PermissionsAuthorizationPolicyName.For(permissions, AllRequired);
    }
}