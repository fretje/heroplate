namespace Heroplate.Api.Contracts.Multitenancy;

public static class MultitenancyConstants
{
    public static class Root
    {
        public const string Id = "root";
        public const string Name = "Root";
        public const string EmailAddress = "admin@root.com";

        // This is the userId that will get assigned to the admin user. Used for testing.
        public const string AdminUserId = "432aaa5e-b0f3-4d8f-9924-f4cbd6e51c27";
    }

    public const string DefaultPassword = "123Pa$$word!";

    public const string TenantIdName = "tenant";
}