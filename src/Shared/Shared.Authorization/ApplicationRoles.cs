using System.Collections.ObjectModel;

namespace Heroplate.Shared.Authorization;

public static class ApplicationRoles
{
    public const string Admin = nameof(Admin);
    public const string Basic = nameof(Basic);

    public static IReadOnlyList<string> DefaultRoles { get; } = new ReadOnlyCollection<string>(new[]
    {
        Admin,
        Basic
    });

    public static bool IsDefault(string? roleName) => DefaultRoles.Any(r => r == roleName);
}