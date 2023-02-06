using Heroplate.Api.Domain.Catalog;

namespace Heroplate.Api.Application.Catalog.Products;

public class GetProductRequest : IRequest<ProductDto>
{
    public int Id { get; set; }
    public GetProductRequest(int id) => Id = id;
}

public class GetProductRequestHandler : IRequestHandler<GetProductRequest, ProductDto>
{
    private readonly IReadRepository<Product> _productRepo;
    private readonly IStringLocalizer _localizer;
    public GetProductRequestHandler(IReadRepository<Product> productRepo, IStringLocalizer<GetProductRequestHandler> localizer) =>
        (_productRepo, _localizer) = (productRepo, localizer);

    public async Task<ProductDto> Handle(GetProductRequest req, CancellationToken ct) =>
        await _productRepo.SingleOrDefaultAsync(new ProductDtoByIdSpec(req.Id), ct)
            ?? throw new NotFoundException(_localizer["Product with Id {0} not found.", req.Id]);
}