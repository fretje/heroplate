using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Heroplate.Shared.Authorization;

public class PermissionsAuthorizationHandler : AuthorizationHandler<PermissionsAuthorizationRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionsAuthorizationRequirement requirement)
    {
        string[] userPermissions = context.User.GetPermissions();

        if ((requirement.AllRequired
                && requirement.Permissions.All(permission => userPermissions.Contains(permission)))
            || (!requirement.AllRequired
                && requirement.Permissions.Any(permission => userPermissions.Contains(permission))))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}