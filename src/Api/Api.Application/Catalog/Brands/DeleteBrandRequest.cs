using Heroplate.Api.Application.Catalog.Products;
using Heroplate.Api.Domain.Catalog;

namespace Heroplate.Api.Application.Catalog.Brands;

public class DeleteBrandRequest : IRequest<int>
{
    public int Id { get; set; }
    public DeleteBrandRequest(int id) => Id = id;
}

public class DeleteBrandRequestHandler : IRequestHandler<DeleteBrandRequest, int>
{
    private readonly IRepositoryWithEvents<Brand> _brandRepo;
    private readonly IReadRepository<Product> _productRepo;
    private readonly IStringLocalizer _localizer;
    public DeleteBrandRequestHandler(IRepositoryWithEvents<Brand> brandRepo, IReadRepository<Product> productRepo, IStringLocalizer<DeleteBrandRequestHandler> localizer) =>
        (_brandRepo, _productRepo, _localizer) = (brandRepo, productRepo, localizer);

    public async Task<int> Handle(DeleteBrandRequest req, CancellationToken ct)
    {
        var brand = await _brandRepo.GetByIdAsync(req.Id, ct)
            ?? throw new NotFoundException(_localizer["Brand with Id {0} not found.", req.Id]);

        if (await _productRepo.AnyAsync(new ProductsByBrandIdSpec(req.Id), ct))
        {
            throw new ConflictException(_localizer["This brand can't be deleted. It still has products."]);
        }

        await _brandRepo.DeleteAsync(brand, ct);

        return req.Id;
    }
}