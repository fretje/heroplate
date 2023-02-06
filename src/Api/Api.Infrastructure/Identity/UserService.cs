using Finbuckle.MultiTenant;
using Heroplate.Api.Application.Common.Events;
using Heroplate.Api.Application.Common.Exceptions;
using Heroplate.Api.Application.Common.FileStorage;
using Heroplate.Api.Application.Common.Interfaces;
using Heroplate.Api.Application.Common.Mailing;
using Heroplate.Api.Application.Identity.Users;
using Heroplate.Api.Domain.Identity;
using Heroplate.Api.Infrastructure.Auth;
using Heroplate.Api.Infrastructure.Persistence.Context;
using Heroplate.Shared.Authorization;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace Heroplate.Api.Infrastructure.Identity;

internal partial class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly ApplicationDbContext _db;
    private readonly IStringLocalizer _t;
    private readonly IFileStorageService _fileStorage;
    private readonly IEventPublisher _events;
    private readonly ITenantInfo _currentTenant;
    private readonly SecuritySettings _securitySettings;
    private readonly IMailService _mailService;
    private readonly IEmailTemplateService _emailTemplates;
    private readonly IBackgroundJobService _backgroundJobs;

    public UserService(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        ApplicationDbContext db,
        IStringLocalizer<UserService> localizer,
        IFileStorageService fileStorage,
        IEventPublisher events,
        ITenantInfo currentTenant,
        IOptions<SecuritySettings> securitySettings,
        IMailService mailService,
        IEmailTemplateService emailTemplates,
        IBackgroundJobService backgroundJobs)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _db = db;
        _t = localizer;
        _fileStorage = fileStorage;
        _events = events;
        _currentTenant = currentTenant;
        _securitySettings = securitySettings.Value;
        _mailService = mailService;
        _emailTemplates = emailTemplates;
        _backgroundJobs = backgroundJobs;
    }

    public async Task<bool> ExistsWithNameAsync(string name)
    {
        EnsureValidTenant();
        return await _userManager.FindByNameAsync(name) is not null;
    }

    public async Task<bool> ExistsWithEmailAsync(string email, string? exceptId = null)
    {
        EnsureValidTenant();
        return await _userManager.FindByEmailAsync(email.Normalize()) is ApplicationUser user && user.Id != exceptId;
    }

    public async Task<bool> ExistsWithPhoneNumberAsync(string phoneNumber, string? exceptId = null)
    {
        EnsureValidTenant();
        return await _userManager.Users.FirstOrDefaultAsync(x => x.PhoneNumber == phoneNumber) is ApplicationUser user && user.Id != exceptId;
    }

    private void EnsureValidTenant()
    {
        if (string.IsNullOrWhiteSpace(_currentTenant?.Id))
        {
            throw new UnauthorizedException(_t["Invalid Tenant."]);
        }
    }

    public async Task<List<UserDetailsDto>> GetListAsync(CancellationToken ct) =>
        (await _userManager.Users
                .AsNoTracking()
                .ToListAsync(ct))
            .Adapt<List<UserDetailsDto>>();

    public Task<int> GetCountAsync(CancellationToken ct) =>
        _userManager.Users.AsNoTracking().CountAsync(ct);

    public async Task<UserDetailsDto> GetAsync(string userId, CancellationToken ct)
    {
        var user = await _userManager.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(_t["User {0} not found.", userId]);

        return user.Adapt<UserDetailsDto>();
    }

    public async Task ToggleStatusAsync(ToggleUserStatusRequest req, CancellationToken ct)
    {
        var user = await _userManager.Users.Where(u => u.Id == req.UserId).FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(_t["User {0} not found.", req.UserId ?? "N/A"]);

        bool isAdmin = await _userManager.IsInRoleAsync(user, ApplicationRoles.Admin);
        if (isAdmin)
        {
            throw new ConflictException(_t["Administrators Profile's Status cannot be toggled"]);
        }

        user.IsActive = req.ActivateUser;

        await _userManager.UpdateAsync(user);

        await _events.PublishAsync(new ApplicationUserUpdatedEvent(user.Id, user.ObjectId), ct);
    }
}