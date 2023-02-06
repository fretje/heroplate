namespace Heroplate.Api.Application.Multitenancy;

public class DeactivateTenantRequest : IRequest<string>
{
    public string TenantId { get; set; } = default!;
    public DeactivateTenantRequest(string tenantId) => TenantId = tenantId;
}

public class DeactivateTenantRequestValidator : AbstractValidator<DeactivateTenantRequest>
{
    public DeactivateTenantRequestValidator() =>
        RuleFor(t => t.TenantId)
            .NotEmpty();
}

public class DeactivateTenantRequestHandler : IRequestHandler<DeactivateTenantRequest, string>
{
    private readonly ITenantService _tenantService;
    public DeactivateTenantRequestHandler(ITenantService tenantService) => _tenantService = tenantService;

    public Task<string> Handle(DeactivateTenantRequest req, CancellationToken ct) =>
        _tenantService.DeactivateAsync(req.TenantId, ct);
}