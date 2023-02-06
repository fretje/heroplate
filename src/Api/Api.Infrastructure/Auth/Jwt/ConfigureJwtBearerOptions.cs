using System.Security.Claims;
using System.Text;
using Finbuckle.MultiTenant;
using Heroplate.Api.Application.Common.Exceptions;
using Heroplate.Api.Infrastructure.Identity;
using Heroplate.Api.Infrastructure.Multitenancy;
using Heroplate.Shared.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Heroplate.Api.Infrastructure.Auth.Jwt;

public class ConfigureJwtBearerOptions : IConfigureNamedOptions<JwtBearerOptions>
{
    private readonly JwtSettings _jwtSettings;
    public ConfigureJwtBearerOptions(IOptions<JwtSettings> jwtSettings) => _jwtSettings = jwtSettings.Value;

    public void Configure(JwtBearerOptions options) =>
        Configure(JwtBearerDefaults.AuthenticationScheme, options);

    public void Configure(string? name, JwtBearerOptions options)
    {
        if (name != JwtBearerDefaults.AuthenticationScheme)
        {
            return;
        }

        byte[] key = Encoding.ASCII.GetBytes(_jwtSettings.Key);

        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateLifetime = true,
            ValidateAudience = false,
            RoleClaimType = ClaimTypes.Role,
            ClockSkew = TimeSpan.Zero
        };
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                if (context.Principal?.Identities.FirstOrDefault() is not { } identity
                     || identity.FindFirst(CustomClaimTypes.Tenant) is not { } tenantClaim
                     || await context.HttpContext.RequestServices.GetRequiredService<ITenantCache>()
                         .GetByIdAsync(tenantClaim.Value) is not { } tenant
                     || identity.FindFirst(ClaimTypes.NameIdentifier) is not { } userIdClaim)
                {
                    throw new UnauthorizedException("Authentication Failed.");
                }

                // Set new tenant info to the HttpContext (for the IUserCache)
                context.HttpContext.TrySetTenantInfo(tenant, true);

                // Add permission claims.
                var userCache = context.HttpContext.RequestServices.GetRequiredService<IUserCache>();
                identity.AddPermissions(
                     await userCache.GetPermissionsAsync(userIdClaim.Value));
            },
            OnChallenge = context =>
            {
                context.HandleResponse();
                return !context.Response.HasStarted ? throw new UnauthorizedException("Authentication Failed.") : Task.CompletedTask;
            },
            OnForbidden = _ => throw new ForbiddenException("You are not authorized to access this resource."),
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
        };
    }
}