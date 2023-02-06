using Heroplate.Api.Domain.Catalog;

namespace Heroplate.Api.Application.Catalog.Brands;

public class GetBrandRequest : IRequest<BrandDto>
{
    public int Id { get; set; }
    public GetBrandRequest(int id) => Id = id;
}

public class GetBrandRequestHandler : IRequestHandler<GetBrandRequest, BrandDto>
{
    private readonly IReadRepository<Brand> _brandRepo;
    private readonly IStringLocalizer _localizer;
    public GetBrandRequestHandler(IReadRepository<Brand> brandRepo, IStringLocalizer<GetBrandRequestHandler> localizer) =>
        (_brandRepo, _localizer) = (brandRepo, localizer);

    public async Task<BrandDto> Handle(GetBrandRequest req, CancellationToken ct) =>
        await _brandRepo.SingleOrDefaultAsync(new EntityDtoByIdSpec<Brand, BrandDto>(req.Id), ct)
            ?? throw new NotFoundException(_localizer["Brand with Id {0} not found.", req.Id]);
}