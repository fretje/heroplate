using Heroplate.Api.Domain.Catalog;

namespace Heroplate.Api.Application.Catalog.Products;

public class DeleteProductRequest : IRequest<int>
{
    public int Id { get; set; }
    public DeleteProductRequest(int id) => Id = id;
}

public class DeleteProductRequestHandler : IRequestHandler<DeleteProductRequest, int>
{
    private readonly IRepositoryWithEvents<Product> _productRepo;
    private readonly IStringLocalizer _localizer;
    public DeleteProductRequestHandler(IRepositoryWithEvents<Product> productRepo, IStringLocalizer<DeleteProductRequestHandler> localizer) =>
        (_productRepo, _localizer) = (productRepo, localizer);

    public async Task<int> Handle(DeleteProductRequest req, CancellationToken ct)
    {
        var product = await _productRepo.GetByIdAsync(req.Id, ct)
            ?? throw new NotFoundException(_localizer["Product with Id {0} not found.", req.Id]);

        await _productRepo.DeleteAsync(product, ct);

        return req.Id;
    }
}