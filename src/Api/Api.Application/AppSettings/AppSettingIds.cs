namespace Heroplate.Api.Application.AppSettings;

public static class AppSettingIds
{
    public static class BackendApi
    {
        public const string ApiUrl = $"{nameof(BackendApi)}:{nameof(ApiUrl)}";
    }

    public static class Exact
    {
        public const string AuthorizationCallbackUrl = $"{nameof(Exact)}:{nameof(AuthorizationCallbackUrl)}";
    }
}