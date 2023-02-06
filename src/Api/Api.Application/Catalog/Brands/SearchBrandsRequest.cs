using Heroplate.Api.Domain.Catalog;

namespace Heroplate.Api.Application.Catalog.Brands;

public class SearchBrandsRequest : PaginationFilter, IRequest<PaginationResponse<BrandDto>>
{
}

public class BrandDtosBySearchRequestSpec : EntityDtosByPaginationFilterSpec<Brand, BrandDto>
{
    public BrandDtosBySearchRequestSpec(SearchBrandsRequest req)
        : base(req) =>
        Query.OrderBy(h => h.Name, !req.HasOrderBy());
}

public class SearchBrandsRequestHandler : IRequestHandler<SearchBrandsRequest, PaginationResponse<BrandDto>>
{
    private readonly IReadRepository<Brand> _brandRepo;
    public SearchBrandsRequestHandler(IReadRepository<Brand> brandRepo) => _brandRepo = brandRepo;

    public Task<PaginationResponse<BrandDto>> Handle(SearchBrandsRequest req, CancellationToken ct) =>
        _brandRepo.PaginatedListAsync(new BrandDtosBySearchRequestSpec(req), ct);
}