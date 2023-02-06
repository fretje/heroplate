using Heroplate.Admin.Infrastructure.Auth.AzureAd;
using Heroplate.Admin.Infrastructure.Auth.Jwt;
using Heroplate.Shared.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Heroplate.Admin.Infrastructure.Auth;

internal static class Startup
{
    public static IServiceCollection AddAuthentication(this IServiceCollection services, IConfiguration config) =>
        config[nameof(AuthProvider)] switch
        {
            // Azure Ad
            nameof(AuthProvider.AzureAd) => services
                .AddScoped<IAuthenticationService, AzureAdAuthenticationService>()
                .AddScoped<AzureAdAuthorizationMessageHandler>()
                .AddMsalAuthentication(options =>
                    {
                        config.Bind(nameof(AuthProvider.AzureAd), options.ProviderOptions.Authentication);
                        options.ProviderOptions.DefaultAccessTokenScopes.Add(
                            config[$"{nameof(AuthProvider.AzureAd)}:{ConfigNames.ApiScope}"] ?? throw new InvalidOperationException("No AzureAd ApiScope defined in app settings."));
                        options.ProviderOptions.LoginMode = "redirect";
                    })
                    .AddAccountClaimsPrincipalFactory<AzureAdClaimsPrincipalFactory>()
                    .Services,

            // Jwt
            _ => services
                .AddScoped<AuthenticationStateProvider, JwtAuthenticationService>()
                .AddScoped(sp => (IAuthenticationService)sp.GetRequiredService<AuthenticationStateProvider>())
                .AddScoped(sp => (IAccessTokenProvider)sp.GetRequiredService<AuthenticationStateProvider>())
                .AddScoped<IAccessTokenProviderAccessor, AccessTokenProviderAccessor>()
                .AddScoped<JwtAuthenticationHeaderHandler>(),
        };

    public static IServiceCollection AddAuthorization(this IServiceCollection services) =>
        services
            .AddAuthorizationCore()
            .AddSingleton<IAuthorizationHandler, PermissionsAuthorizationHandler>()
            .AddSingleton<IAuthorizationPolicyProvider, PermissionsAuthorizationPolicyProvider>();

    public static IHttpClientBuilder AddAuthorizationHandler(this IHttpClientBuilder builder, IConfiguration config) =>
        config[nameof(AuthProvider)] switch
        {
            // Azure Ad
            nameof(AuthProvider.AzureAd) =>
                builder.AddHttpMessageHandler<AzureAdAuthorizationMessageHandler>(),

            // Jwt
            _ => builder.AddHttpMessageHandler<JwtAuthenticationHeaderHandler>()
        };
}