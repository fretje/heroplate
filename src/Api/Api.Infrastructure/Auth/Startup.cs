using Heroplate.Api.Application.Common.Interfaces;
using Heroplate.Api.Infrastructure.Auth.AzureAd;
using Heroplate.Api.Infrastructure.Auth.FakeJwt;
using Heroplate.Api.Infrastructure.Auth.Jwt;
using Heroplate.Api.Infrastructure.Identity;
using Heroplate.Shared.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Heroplate.Api.Infrastructure.Auth;

internal static class Startup
{
    internal static IServiceCollection AddAuth(this IServiceCollection services, IConfiguration config)
    {
        services
            .AddCurrentUser()
            .AddPermissions()
            .AddIdentity(); // Must add identity before adding auth!

        services.Configure<SecuritySettings>(config.GetSection(nameof(SecuritySettings)));
        return config["SecuritySettings:Provider"]?.Equals("AzureAd", StringComparison.OrdinalIgnoreCase) is true
            ? services.AddAzureAdAuth(config)
            : config["SecuritySettings:Provider"]?.Equals("FakeJwt", StringComparison.OrdinalIgnoreCase) is true
            ? services.AddFakeJwtAuth()
            : services.AddJwtAuth();
    }

    internal static IApplicationBuilder UseCurrentUser(this IApplicationBuilder app) =>
        app.UseMiddleware<CurrentUserMiddleware>();

    private static IServiceCollection AddCurrentUser(this IServiceCollection services) =>
        services
            .AddScoped<CurrentUserMiddleware>()
            .AddScoped<ICurrentUser, CurrentUser>()
            .AddScoped(sp => (ICurrentUserInitializer)sp.GetRequiredService<ICurrentUser>());

    private static IServiceCollection AddPermissions(this IServiceCollection services) =>
        services
            .AddAuthorization()
            .AddSingleton<IAuthorizationPolicyProvider, PermissionsAuthorizationPolicyProvider>()
            .AddSingleton<IAuthorizationHandler, PermissionsAuthorizationHandler>();
}