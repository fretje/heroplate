using Heroplate.Api.Domain.Catalog;

namespace Heroplate.Api.Application.Catalog.Products;

public class ProductsByBrandIdSpec : Specification<Product>
{
    public ProductsByBrandIdSpec(int brandId) =>
        Query.Where(g => g.BrandId == brandId);
}