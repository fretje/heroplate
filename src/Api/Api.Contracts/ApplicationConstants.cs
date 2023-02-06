namespace Heroplate.Api.Contracts;

public static class ApplicationConstants
{
    public const string ApiUrl = "/api/v1/";

    public const string TenantIdentifierHeader = "tenant";
    public const string VersionHeader = "version";

    public const string InvokeRequestMethod = "InvokeRequest";
    public const string SignalInvokeSucceededMethod = "SignalInvokeSucceeded";
    public const string SignalInvokeFailedMethod = "SignalInvokeFailed";
}