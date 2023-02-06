using Heroplate.Api.Domain.Catalog;

namespace Heroplate.Api.Application.Catalog.Products;

public class SearchProductsRequest : PaginationFilter, IRequest<PaginationResponse<ProductDto>>
{
    public int? BrandId { get; set; }
    public decimal? MinimumRate { get; set; }
    public decimal? MaximumRate { get; set; }
}

public class ProductDtosBySearchRequestSpec : EntityDtosByPaginationFilterSpec<Product, ProductDto>
{
    public ProductDtosBySearchRequestSpec(SearchProductsRequest req)
        : base(req) =>
        Query
            .Where(g => g.BrandId == req.BrandId!.Value, req.BrandId.HasValue)
            .Where(p => p.Rate >= req.MinimumRate!.Value, req.MinimumRate.HasValue)
            .Where(p => p.Rate <= req.MaximumRate!.Value, req.MaximumRate.HasValue)
            .OrderBy(g => g.Brand.Name, !req.HasOrderBy())
                .ThenBy(g => g.Name, !req.HasOrderBy())
            .Include(g => g.Brand);
}

public class SearchProductsRequestHandler : IRequestHandler<SearchProductsRequest, PaginationResponse<ProductDto>>
{
    private readonly IReadRepository<Product> _productRepo;
    public SearchProductsRequestHandler(IReadRepository<Product> productRepo) => _productRepo = productRepo;

    public Task<PaginationResponse<ProductDto>> Handle(SearchProductsRequest req, CancellationToken ct) =>
        _productRepo.PaginatedListAsync(new ProductDtosBySearchRequestSpec(req), ct);
}