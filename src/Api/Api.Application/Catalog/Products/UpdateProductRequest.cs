using Heroplate.Api.Domain.Catalog;

namespace Heroplate.Api.Application.Catalog.Products;

public class UpdateProductRequest : CreateProductRequest, IUpdateEntityRequest
{
    public int Id { get; set; }
    public bool DeleteCurrentImage { get; set; }
}

public class UpdateProductRequestValidator : AbstractValidator<UpdateProductRequest>
{
    public UpdateProductRequestValidator(IReadRepository<Product> productRepo, IReadRepository<Brand> brandRepo, IValidator<FileUploadRequest> imageValidator, IStringLocalizer<CreateProductRequestValidator> localizer) =>
        Include(new CreateProductRequestValidator(productRepo, brandRepo, imageValidator, localizer));
}

public class UpdateProductRequestHandler : IRequestHandler<UpdateProductRequest, int>
{
    private readonly IRepositoryWithEvents<Product> _productRepo;
    private readonly IStringLocalizer _localizer;
    private readonly IFileStorageService _fileStorage;
    public UpdateProductRequestHandler(IRepositoryWithEvents<Product> productRepo, IStringLocalizer<UpdateProductRequestHandler> localizer, IFileStorageService fileStorage) =>
        (_productRepo, _localizer, _fileStorage) = (productRepo, localizer, fileStorage);

    public async Task<int> Handle(UpdateProductRequest req, CancellationToken ct)
    {
        var product = await _productRepo.GetByIdAsync(req.Id, ct)
            ?? throw new NotFoundException(_localizer["Product with Id {0} not found.", req.Id]);

        // Remove old image if flag is set
        if (req.DeleteCurrentImage)
        {
            string? currentProductImagePath = product.ImagePath;
            if (!string.IsNullOrEmpty(currentProductImagePath))
            {
                string root = Directory.GetCurrentDirectory();
                _fileStorage.Remove(Path.Combine(root, currentProductImagePath));
            }

            product.ClearImagePath();
        }

        string? productImagePath = req.Image is not null
            ? await _fileStorage.UploadAsync<Product>(req.Image, FileType.Image, ct)
            : null;

        product.Update(req.Name, req.Description, req.Rate, productImagePath, req.BrandId);

        await _productRepo.UpdateAsync(product, ct);

        return req.Id;
    }
}