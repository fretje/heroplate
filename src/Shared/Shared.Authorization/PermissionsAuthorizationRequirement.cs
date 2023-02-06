using Microsoft.AspNetCore.Authorization;

namespace Heroplate.Shared.Authorization;

public class PermissionsAuthorizationRequirement : IAuthorizationRequirement
{
    public string[] Permissions { get; private set; }

    /// <summary>
    /// When true, all of the provided permissions are required for the requirement to be fulfilled.
    /// When false, only one of the provided permissions is required for the requirement to be fulfilled.
    /// </summary>
    public bool AllRequired { get; private set; }

    public PermissionsAuthorizationRequirement(string[] permissions, bool allRequired) =>
        (Permissions, AllRequired) = (permissions, allRequired);
}