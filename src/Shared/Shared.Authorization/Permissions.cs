namespace Heroplate.Shared.Authorization;

public static class Permissions
{
    public static class Tenants
    {
        public const string View = $"{nameof(Tenants)}.{nameof(View)}";
        public const string Create = $"{nameof(Tenants)}.{nameof(Create)}";
        public const string Update = $"{nameof(Tenants)}.{nameof(Update)}";
    }

    public static class Dashboard
    {
        public const string View = $"{nameof(Dashboard)}.{nameof(View)}";
    }

    public static class About
    {
        public const string View = $"{nameof(About)}.{nameof(View)}";
    }

    public static class Hangfire
    {
        public const string View = $"{nameof(Hangfire)}.{nameof(View)}";
    }

    public static class Users
    {
        public const string View = $"{nameof(Users)}.{nameof(View)}";
        public const string Create = $"{nameof(Users)}.{nameof(Create)}";
        public const string Update = $"{nameof(Users)}.{nameof(Update)}";
        public const string Delete = $"{nameof(Users)}.{nameof(Delete)}";
        public const string Export = $"{nameof(Users)}.{nameof(Export)}";
    }

    public static class UserRoles
    {
        public const string View = $"{nameof(UserRoles)}.{nameof(View)}";
        public const string Update = $"{nameof(UserRoles)}.{nameof(Update)}";
    }

    public static class Roles
    {
        public const string View = $"{nameof(Roles)}.{nameof(View)}";
        public const string Create = $"{nameof(Roles)}.{nameof(Create)}";
        public const string Update = $"{nameof(Roles)}.{nameof(Update)}";
        public const string Delete = $"{nameof(Roles)}.{nameof(Delete)}";
    }

    public static class RoleClaims
    {
        public const string View = $"{nameof(RoleClaims)}.{nameof(View)}";
        public const string Update = $"{nameof(RoleClaims)}.{nameof(Update)}";
    }

    public static class AppSettings
    {
        public const string View = $"{nameof(AppSettings)}.{nameof(View)}";
        public const string Update = $"{nameof(AppSettings)}.{nameof(Update)}";
    }

    public static class Brands
    {
        public const string View = $"{nameof(Brands)}.{nameof(View)}";
        public const string Create = $"{nameof(Brands)}.{nameof(Create)}";
        public const string Update = $"{nameof(Brands)}.{nameof(Update)}";
        public const string Delete = $"{nameof(Brands)}.{nameof(Delete)}";
    }

    public static class Products
    {
        public const string View = $"{nameof(Products)}.{nameof(View)}";
        public const string Create = $"{nameof(Products)}.{nameof(Create)}";
        public const string Update = $"{nameof(Products)}.{nameof(Update)}";
        public const string Delete = $"{nameof(Products)}.{nameof(Delete)}";
    }

    internal static string ResourceFor(string permission) =>
        permission.Split('.') is { } permissionParts && permissionParts.Length > 0
            ? permissionParts[0]
            : throw new ArgumentException("Not a valid permission", nameof(permission));
}