using Heroplate.Api.Application.Multitenancy;

namespace Heroplate.Api.Host.Controllers.Multitenancy;

public class TenantsController : VersionNeutralApiController
{
    [HttpGet]
    [PermissionsAuthorize(Permissions.Tenants.View)]
    [OpenApiOperation("Get a list of all tenants.", "")]
    public Task<List<TenantDto>> GetListAsync(CancellationToken ct)
    {
        return Mediator.Send(new GetAllTenantsRequest(), ct);
    }

    [HttpGet("{id}")]
    [PermissionsAuthorize(Permissions.Tenants.View)]
    [OpenApiOperation("Get tenant details.", "")]
    public Task<TenantDto> GetAsync(string id, CancellationToken ct)
    {
        return Mediator.Send(new GetTenantRequest(id), ct);
    }

    [HttpPost]
    [PermissionsAuthorize(Permissions.Tenants.Create)]
    [OpenApiOperation("Create a new tenant.", "")]
    public Task<string> CreateAsync(CreateTenantRequest req, CancellationToken ct)
    {
        return Mediator.Send(req, ct);
    }

    [HttpPost("{id}/activate")]
    [PermissionsAuthorize(Permissions.Tenants.Update)]
    [OpenApiOperation("Activate a tenant.", "")]
    [ApiConventionMethod(typeof(BaseApiConventions), nameof(BaseApiConventions.BaseGet))]
    public Task<string> ActivateAsync(string id, CancellationToken ct)
    {
        return Mediator.Send(new ActivateTenantRequest(id), ct);
    }

    [HttpPost("{id}/deactivate")]
    [PermissionsAuthorize(Permissions.Tenants.Update)]
    [OpenApiOperation("Deactivate a tenant.", "")]
    [ApiConventionMethod(typeof(BaseApiConventions), nameof(BaseApiConventions.BaseGet))]
    public Task<string> DeactivateAsync(string id, CancellationToken ct)
    {
        return Mediator.Send(new DeactivateTenantRequest(id), ct);
    }

    [HttpPost("{id}/upgrade")]
    [PermissionsAuthorize(Permissions.Tenants.Update)]
    [OpenApiOperation("Upgrade a tenant's subscription.", "")]
    [ApiConventionMethod(typeof(BaseApiConventions), nameof(BaseApiConventions.Search))]
    public async Task<ActionResult<string>> UpgradeSubscriptionAsync(string id, UpgradeSubscriptionRequest req, CancellationToken ct)
    {
        return id != req.TenantId
            ? BadRequest()
            : Ok(await Mediator.Send(req, ct));
    }
}