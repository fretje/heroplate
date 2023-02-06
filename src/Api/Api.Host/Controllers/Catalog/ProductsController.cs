using Heroplate.Api.Application.Catalog.Products;
using Heroplate.Api.Application.Common.Models;

namespace Heroplate.Api.Host.Controllers.Catalog;

public class ProductsController : VersionedApiController
{
    [HttpPost("search")]
    [PermissionsAuthorize(Permissions.Products.View)]
    [OpenApiOperation("Search products using available filters.", "")]
    public Task<PaginationResponse<ProductDto>> SearchAsync(SearchProductsRequest req, CancellationToken ct)
    {
        return Mediator.Send(req, ct);
    }

    [HttpGet("{id:int}")]
    [PermissionsAuthorize(Permissions.Products.View)]
    [OpenApiOperation("Get product details.", "")]
    public Task<ProductDto> GetAsync(int id, CancellationToken ct)
    {
        return Mediator.Send(new GetProductRequest(id), ct);
    }

    [HttpPost]
    [PermissionsAuthorize(Permissions.Products.Create)]
    [OpenApiOperation("Create a new product.", "")]
    public Task<int> CreateAsync(CreateProductRequest req, CancellationToken ct)
    {
        return Mediator.Send(req, ct);
    }

    [HttpPut("{id:int}")]
    [PermissionsAuthorize(Permissions.Products.Update)]
    [OpenApiOperation("Update a product.", "")]
    public async Task<ActionResult<int>> UpdateAsync(int id, UpdateProductRequest req, CancellationToken ct)
    {
        return id != req.Id ? BadRequest() : Ok(await Mediator.Send(req, ct));
    }

    [HttpDelete("{id:int}")]
    [PermissionsAuthorize(Permissions.Products.Delete)]
    [OpenApiOperation("Delete a product.", "")]
    public Task<int> DeleteAsync(int id, CancellationToken ct)
    {
        return Mediator.Send(new DeleteProductRequest(id), ct);
    }
}