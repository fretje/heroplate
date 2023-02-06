using System.Collections.ObjectModel;

namespace Heroplate.Shared.Authorization;

public static class PermissionProvider
{
    private static readonly PermissionDto[] _all = new PermissionDto[]
    {
        new(Permissions.Tenants.View,   "View Tenants", IsRoot: true),
        new(Permissions.Tenants.Create, "Create Tenants", IsRoot: true),
        new(Permissions.Tenants.Update, "Update Tenants", IsRoot: true),

        new(Permissions.Dashboard.View, "View Dashboard", IsBasic: true),

        new(Permissions.About.View, "View About"),

        new(Permissions.Hangfire.View, "View Hangfire"),

        new(Permissions.Users.View,   "View Users"),
        new(Permissions.Users.Create, "Create Users"),
        new(Permissions.Users.Update, "Update Users"),
        new(Permissions.Users.Delete, "Delete Users"),
        new(Permissions.Users.Export, "Export Users"),

        new(Permissions.UserRoles.View,   "View UserRoles"),
        new(Permissions.UserRoles.Update, "Update UserRoles"),

        new(Permissions.Roles.View,   "View Roles"),
        new(Permissions.Roles.Create, "Create Roles"),
        new(Permissions.Roles.Update, "Update Roles"),
        new(Permissions.Roles.Delete, "Delete Roles"),

        new(Permissions.RoleClaims.View,   "View RoleClaims"),
        new(Permissions.RoleClaims.Update, "Update RoleClaims"),

        new(Permissions.AppSettings.View,   "View AppSettings"),
        new(Permissions.AppSettings.Update, "Update AppSettings"),

        new(Permissions.Brands.View,   "View Brands", IsBasic: true),
        new(Permissions.Brands.Create, "Create Brands"),
        new(Permissions.Brands.Update, "Update Brands"),
        new(Permissions.Brands.Delete, "Delete Brands"),

        new(Permissions.Products.View,   "View Products", IsBasic: true),
        new(Permissions.Products.Create, "Create Products"),
        new(Permissions.Products.Update, "Update Products"),
        new(Permissions.Products.Delete, "Delete Products"),
    };

    public static IReadOnlyList<PermissionDto> AllPermissions { get; } = new ReadOnlyCollection<PermissionDto>(_all);
    public static IReadOnlyList<PermissionDto> RootPermissions { get; } = new ReadOnlyCollection<PermissionDto>(_all.Where(p => p.IsRoot).ToArray());
    public static IReadOnlyList<PermissionDto> AdminPermissions { get; } = new ReadOnlyCollection<PermissionDto>(_all.Where(p => !p.IsRoot).ToArray());
    public static IReadOnlyList<PermissionDto> BasicPermissions { get; } = new ReadOnlyCollection<PermissionDto>(_all.Where(p => p.IsBasic).ToArray());

    public static string[] All { get; } = AllPermissions.Select(p => p.Name).ToArray();
    public static string[] Root { get; } = RootPermissions.Select(p => p.Name).ToArray();
    public static string[] Admin { get; } = AdminPermissions.Select(p => p.Name).ToArray();
    public static string[] Basic { get; } = BasicPermissions.Select(p => p.Name).ToArray();
}