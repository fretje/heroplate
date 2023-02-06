using System.Security.Claims;
using Heroplate.Api.Application.Common.Events;
using Heroplate.Api.Application.Common.Exceptions;
using Heroplate.Api.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Microsoft.Identity.Web;

namespace Heroplate.Api.Infrastructure.Identity;

internal class CreateUserService : ICreateUserService
{
    private readonly IUserCache _userCache;
    private readonly IStringLocalizer _t;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IEventPublisher _events;
    public CreateUserService(IUserCache userCache, IStringLocalizer<CreateUserService> t, UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, IEventPublisher events)
    {
        _userCache = userCache;
        _t = t;
        _userManager = userManager;
        _roleManager = roleManager;
        _events = events;
    }

    /// <summary>
    /// This is used when authenticating with AzureAd.
    /// The local user is retrieved using the objectidentifier claim present in the ClaimsPrincipal.
    /// If no such claim is found, an InternalServerException is thrown.
    /// If no user is found with that ObjectId, a new one is created and populated with the values from the ClaimsPrincipal.
    /// If a role claim is present in the principal, and the user is not yet in that role, then the user is added to that role.
    /// </summary>
    public async Task<string> GetOrCreateFromPrincipalAsync(ClaimsPrincipal principal, CancellationToken ct)
    {
        string? objectId = principal.GetObjectId();
        if (string.IsNullOrWhiteSpace(objectId))
        {
            throw new InternalServerException(_t["Invalid objectId"]);
        }

        var user = await _userCache.GetByObjectIdAsync(objectId, ct)
            ?? await CreateOrUpdateFromPrincipalAsync(principal, ct);

        if (principal.FindFirstValue(ClaimTypes.Role) is string role &&
            !await _userManager.IsInRoleAsync(user, role) &&
            await _roleManager.RoleExistsAsync(role))
        {
            await _userManager.AddToRoleAsync(user, role);
        }

        return user.Id;
    }

    private async Task<ApplicationUser> CreateOrUpdateFromPrincipalAsync(ClaimsPrincipal principal, CancellationToken ct)
    {
        string? email = principal.FindFirstValue(ClaimTypes.Upn);
        string? userName = principal.GetDisplayName();
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(userName))
        {
            throw new InternalServerException(_t["Username or Email not valid."]);
        }

        var user = await _userManager.FindByNameAsync(userName);
        if (user is not null && !string.IsNullOrWhiteSpace(user.ObjectId))
        {
            throw new InternalServerException(_t["Username {0} is already taken.", userName]);
        }

        if (user is null)
        {
            user = await _userManager.FindByEmailAsync(email);
            if (user is not null && !string.IsNullOrWhiteSpace(user.ObjectId))
            {
                throw new InternalServerException(_t["Email {0} is already taken.", email]);
            }
        }

        IdentityResult? result;
        if (user is not null)
        {
            user.ObjectId = principal.GetObjectId();
            result = await _userManager.UpdateAsync(user);

            await _events.PublishAsync(new ApplicationUserUpdatedEvent(user.Id, user.ObjectId), ct);
        }
        else
        {
            user = new ApplicationUser
            {
                ObjectId = principal.GetObjectId(),
                FirstName = principal.FindFirstValue(ClaimTypes.GivenName),
                LastName = principal.FindFirstValue(ClaimTypes.Surname),
                Email = email,
                NormalizedEmail = email.ToUpperInvariant(),
                UserName = userName,
                NormalizedUserName = userName.ToUpperInvariant(),
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                IsActive = true
            };
            result = await _userManager.CreateAsync(user);

            await _events.PublishAsync(new ApplicationUserCreatedEvent(user.Id), ct);
        }

        return result.Succeeded ? user
            : throw new InternalServerException(_t["Validation Errors Occurred."], result.GetErrors(_t));
    }
}