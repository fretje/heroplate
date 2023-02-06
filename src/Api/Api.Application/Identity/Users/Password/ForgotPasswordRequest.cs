namespace Heroplate.Api.Application.Identity.Users.Password;

public class ForgotPasswordRequest
{
    public string Email { get; set; } = default!;
}

public class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequest>
{
    public ForgotPasswordRequestValidator(IStringLocalizer<ForgotPasswordRequestValidator> localizer) =>
        RuleFor(p => p.Email).Cascade(CascadeMode.Stop)
            .NotEmpty()
            .EmailAddress()
                .WithMessage(localizer["Invalid Email Address."]);
}