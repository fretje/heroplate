using Heroplate.Api.Application.Catalog.Brands;
using Heroplate.Api.Application.Common.Models;

namespace Heroplate.Api.Host.Controllers.Catalog;

public class BrandsController : VersionedApiController
{
    [HttpPost("search")]
    [PermissionsAuthorize(Permissions.Brands.View)]
    [OpenApiOperation("Search brands using available filters.", "")]
    public Task<PaginationResponse<BrandDto>> SearchAsync(SearchBrandsRequest req, CancellationToken ct)
    {
        return Mediator.Send(req, ct);
    }

    [HttpGet("{id:int}")]
    [PermissionsAuthorize(Permissions.Brands.View)]
    [OpenApiOperation("Get brand details.", "")]
    public Task<BrandDto> GetAsync(int id, CancellationToken ct)
    {
        return Mediator.Send(new GetBrandRequest(id), ct);
    }

    [HttpPost]
    [PermissionsAuthorize(Permissions.Brands.Create)]
    [OpenApiOperation("Create a new brand.", "")]
    public Task<int> CreateAsync(CreateBrandRequest req, CancellationToken ct)
    {
        return Mediator.Send(req, ct);
    }

    [HttpPut("{id:int}")]
    [PermissionsAuthorize(Permissions.Brands.Update)]
    [OpenApiOperation("Update a brand.", "")]
    public async Task<ActionResult<int>> UpdateAsync(int id, UpdateBrandRequest req, CancellationToken ct)
    {
        return id != req.Id ? BadRequest() : Ok(await Mediator.Send(req, ct));
    }

    [HttpDelete("{id:int}")]
    [PermissionsAuthorize(Permissions.Brands.Delete)]
    [OpenApiOperation("Delete a brand.", "")]
    public Task<int> DeleteAsync(int id, CancellationToken ct)
    {
        return Mediator.Send(new DeleteBrandRequest(id), ct);
    }
}