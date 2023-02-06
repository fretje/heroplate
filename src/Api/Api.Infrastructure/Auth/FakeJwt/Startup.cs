using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using WebMotions.Fake.Authentication.JwtBearer;

namespace Heroplate.Api.Infrastructure.Auth.FakeJwt;

internal static class Startup
{
    // For testing
    internal static IServiceCollection AddFakeJwtAuth(this IServiceCollection services) =>
        services
            .AddAuthenticationCore(config => config.DefaultAuthenticateScheme = config.DefaultChallengeScheme = FakeJwtBearerDefaults.AuthenticationScheme)
            .AddAuthentication(FakeJwtBearerDefaults.AuthenticationScheme)
                .AddFakeJwtBearer(options =>
                    options.Events = new()
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];

                            if (!string.IsNullOrEmpty(accessToken) &&
                                context.HttpContext.Request.Path.StartsWithSegments("/notifications"))
                            {
                                // Read the token out of the query string
                                context.Token = accessToken;
                            }

                            return Task.CompletedTask;
                        }
                    })
                .Services
            .AddTransient<IClaimsTransformation, FakeJwtClaimsTransformation>();
}

internal class FakeJwtClaimsTransformation : IClaimsTransformation
{
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (!principal.HasClaim(c => c.Type == ClaimTypes.Name))
        {
            var emailClaim = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
            if (emailClaim is not null)
            {
                var claimsIdentity = new ClaimsIdentity();
                principal.Identities.First().AddClaim(new Claim(ClaimTypes.Name, emailClaim.Value));
            }
        }

        return Task.FromResult(principal);
    }
}