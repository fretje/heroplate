using Heroplate.Api.Domain.Catalog;

namespace Heroplate.Api.Application.Catalog.Products;

public class ProductDtoByIdSpec : EntityDtoByIdSpec<Product, ProductDto>
{
    public ProductDtoByIdSpec(int id)
        : base(id) =>
        Query.Include(g => g.Brand);
}