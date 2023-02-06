using Heroplate.Api.Application.Common.Exceptions;
using Heroplate.Api.Application.Common.FileStorage;
using Heroplate.Api.Application.Common.Mailing;
using Heroplate.Api.Application.Identity.Users;
using Heroplate.Api.Domain.Identity;
using Heroplate.Shared.Authorization;

namespace Heroplate.Api.Infrastructure.Identity;

internal partial class UserService
{
    public async Task<string> CreateAsync(CreateUserRequest request, string origin, CancellationToken ct)
    {
        var user = new ApplicationUser
        {
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            UserName = request.UserName,
            PhoneNumber = request.PhoneNumber,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            throw new InternalServerException(_t["Validation Errors Occurred."], result.GetErrors(_t));
        }

        await _userManager.AddToRoleAsync(user, ApplicationRoles.Basic);

        var messages = new List<string> { string.Format(_t["User {0} Registered."], user.UserName) };

        if (_securitySettings.RequireConfirmedAccount && !string.IsNullOrEmpty(user.Email))
        {
            // send verification email
            string emailVerificationUri = await GetEmailVerificationUriAsync(user, origin);
            var eMailModel = new RegisterUserEmailModel()
            {
                Email = user.Email,
                UserName = user.UserName,
                Url = emailVerificationUri
            };
            var mailRequest = new MailRequest(
                new List<string> { user.Email },
                _t["Confirm Registration"],
                _emailTemplates.GenerateEmailTemplate("email-confirmation", eMailModel));
            _backgroundJobs.Enqueue(() => _mailService.SendAsync(mailRequest, CancellationToken.None));
            messages.Add(_t[$"Please check {user.Email} to verify your account!"]);
        }

        await _events.PublishAsync(new ApplicationUserCreatedEvent(user.Id), ct);

        return string.Join(Environment.NewLine, messages);
    }

    public async Task UpdateAsync(UpdateUserRequest req, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(req.Id)
            ?? throw new NotFoundException(_t["User Not Found."]);

        string currentImage = user.ImageUrl ?? "";
        if (req.Image != null || req.DeleteCurrentImage)
        {
            user.ImageUrl = await _fileStorage.UploadAsync<ApplicationUser>(req.Image, FileType.Image, ct);
            if (req.DeleteCurrentImage && !string.IsNullOrEmpty(currentImage))
            {
                string root = Directory.GetCurrentDirectory();
                _fileStorage.Remove(Path.Combine(root, currentImage));
            }
        }

        user.FirstName = req.FirstName;
        user.LastName = req.LastName;
        user.PhoneNumber = req.PhoneNumber;
        string? phoneNumber = await _userManager.GetPhoneNumberAsync(user);
        if (req.PhoneNumber != phoneNumber)
        {
            await _userManager.SetPhoneNumberAsync(user, req.PhoneNumber);
        }

        var result = await _userManager.UpdateAsync(user);

        await _events.PublishAsync(new ApplicationUserUpdatedEvent(user.Id, user.ObjectId), ct);

        if (!result.Succeeded)
        {
            throw new InternalServerException(_t["Update profile failed"], result.GetErrors(_t));
        }
    }
}