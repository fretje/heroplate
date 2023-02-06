namespace Heroplate.Api.Application.Multitenancy;

public class GetAllTenantsRequest : IRequest<List<TenantDto>>
{
}

public class GetAllTenantsRequestHandler : IRequestHandler<GetAllTenantsRequest, List<TenantDto>>
{
    private readonly ITenantService _tenantService;
    public GetAllTenantsRequestHandler(ITenantService tenantService) => _tenantService = tenantService;

    public Task<List<TenantDto>> Handle(GetAllTenantsRequest req, CancellationToken ct) =>
        _tenantService.GetAllAsync();
}