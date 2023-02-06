using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.Extensions.Options;

namespace Heroplate.Admin.Infrastructure.Auth.AzureAd;

internal sealed class AzureAdAuthenticationService : IAuthenticationService
{
    private readonly NavigationManager _navigation;
    private readonly IOptionsSnapshot<RemoteAuthenticationOptions<ApiAuthorizationProviderOptions>> _authOptions;
    public AzureAdAuthenticationService(NavigationManager navigation, IOptionsSnapshot<RemoteAuthenticationOptions<ApiAuthorizationProviderOptions>> authOptions) =>
        (_navigation, _authOptions) = (navigation, authOptions);

    public AuthProvider ProviderType => AuthProvider.AzureAd;

    public void NavigateToLogin(string? returnUrl = null) =>
        _navigation.NavigateToLogin("authentication/login", new InteractiveRequestOptions { Interaction = InteractionType.SignIn, ReturnUrl = returnUrl });

    public Task LogoutAsync()
    {
        _navigation.NavigateToLogout("authentication/logout", _authOptions.Get(Options.DefaultName).AuthenticationPaths.LogOutSucceededPath);
        return Task.CompletedTask;
    }

    public Task<bool> LoginAsync(string tenantId, TokenRequest request) => throw new NotImplementedException();
}