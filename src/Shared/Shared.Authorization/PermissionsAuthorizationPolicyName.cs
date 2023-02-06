using System.Diagnostics.CodeAnalysis;

namespace Heroplate.Shared.Authorization;

public static class PermissionsAuthorizationPolicyName
{
    public const string Prefix = "PermissionsAuthorizationPolicy";

    [return: NotNullIfNotNull("permissions")]
    public static string? For(string[]? permissions, bool allRequired) =>
        permissions is not null
            ? $"{Prefix}^{string.Join('|', permissions)}^{allRequired}"
            : null;

    public static bool TryParse(string? policyName, [NotNullWhen(true)] out string[]? permissions, out bool allRequired)
    {
        if (policyName?.Split('^') is { } nameParts
            && nameParts.Length == 3
            && nameParts[0] == Prefix
            && nameParts[1].Split('|', StringSplitOptions.RemoveEmptyEntries) is { } parsedPermissions
            && bool.TryParse(nameParts[2], out bool parsedAllRequired))
        {
            permissions = parsedPermissions;
            allRequired = parsedAllRequired;
            return true;
        }

        permissions = default;
        allRequired = default;
        return false;
    }

    public static string[] PermissionsFrom(string? policyName) =>
        TryParse(policyName, out string[]? permissions, out _)
            ? permissions
            : Array.Empty<string>();

    public static bool AllRequiredFrom(string? policyName) =>
        TryParse(policyName, out _, out bool allRequired) && allRequired;
}