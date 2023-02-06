namespace Heroplate.Api.Application.Multitenancy;

public class CreateTenantRequestValidator : AbstractValidator<CreateTenantRequest>
{
    public CreateTenantRequestValidator(
        ITenantService tenantService,
        IConnectionStringValidator connectionStringValidator,
        IStringLocalizer<CreateTenantRequestValidator> localizer)
    {
        RuleFor(t => t.Id).Cascade(CascadeMode.Stop)
            .NotEmpty()
            .MustAsync(async (id, _) => !await tenantService.ExistsWithIdAsync(id))
                .WithMessage((_, id) => localizer["Tenant {0} already exists.", id]);

        RuleFor(t => t.Name).Cascade(CascadeMode.Stop)
            .NotEmpty()
            .MustAsync(async (name, _) => !await tenantService.ExistsWithNameAsync(name))
                .WithMessage((_, name) => localizer["Tenant {0} already exists.", name]);

        RuleFor(t => t.ConnectionString).Cascade(CascadeMode.Stop)
            .Must((_, cs) => string.IsNullOrWhiteSpace(cs) || connectionStringValidator.TryValidate(cs))
                .WithMessage(localizer["Connection string invalid."]);

        RuleFor(t => t.AdminEmail).Cascade(CascadeMode.Stop)
            .NotEmpty()
            .EmailAddress();
    }
}