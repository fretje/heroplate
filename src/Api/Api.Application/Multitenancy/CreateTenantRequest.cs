namespace Heroplate.Api.Application.Multitenancy;

public class CreateTenantRequest : IRequest<string>
{
    public string Id { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? ConnectionString { get; set; }
    public string AdminEmail { get; set; } = default!;
    public string? Issuer { get; set; }
}

public class CreateTenantRequestHandler : IRequestHandler<CreateTenantRequest, string>
{
    private readonly ITenantService _tenantService;
    public CreateTenantRequestHandler(ITenantService tenantService) => _tenantService = tenantService;

    public Task<string> Handle(CreateTenantRequest req, CancellationToken ct) =>
        _tenantService.CreateAsync(req, ct);
}