namespace Heroplate.Api.Application.Identity.Users.Password;

public class ChangePasswordRequest
{
    public string Password { get; set; } = default!;
    public string NewPassword { get; set; } = default!;
    public string ConfirmNewPassword { get; set; } = default!;
}

public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator(IStringLocalizer<ChangePasswordRequestValidator> localizer)
    {
        RuleFor(p => p.Password)
            .NotEmpty();

        RuleFor(p => p.NewPassword)
            .NotEmpty();

        RuleFor(p => p.ConfirmNewPassword)
            .Equal(p => p.NewPassword)
                .WithMessage(localizer["Passwords do not match."]);
    }
}