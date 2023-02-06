namespace Heroplate.Api.Application.Catalog.Products;

public class ProductDto : EntityWithNameDto
{
    public decimal Rate { get; set; }
    public string? ImagePath { get; set; }
    public int BrandId { get; set; }
    public string BrandName { get; set; } = default!;
}