namespace Heroplate.Admin.Infrastructure.Auth;

public interface IAuthenticationService
{
    AuthProvider ProviderType { get; }

    void NavigateToLogin(string? returnUrl = null);

    Task<bool> LoginAsync(string tenantId, TokenRequest request);
    Task LogoutAsync();
}