using Heroplate.Api.Domain.Catalog;

namespace Heroplate.Api.Application.Catalog.Products;

public class CreateProductRequest : EntityWithNameRequest
{
    public decimal Rate { get; set; }
    public FileUploadRequest? Image { get; set; }
    public int BrandId { get; set; }
}

public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator(IReadRepository<Product> productRepo, IReadRepository<Brand> brandRepo, IValidator<FileUploadRequest> imageValidator, IStringLocalizer<CreateProductRequestValidator> localizer)
    {
        RuleFor(g => g.Name)
            .NotEmpty()
            .MaximumLength(256)
            .MustAsync(async (product, name, ct) => await productRepo.FirstOrDefaultAsync(new ProductByBrandAndNameSpec(product.BrandId, name), ct) is not { } existingProduct
                        || (product is IUpdateEntityRequest entityToUpdate && existingProduct.Id == entityToUpdate.Id))
                .WithMessage((_, name) => localizer["A Product with the name '{0}' already exists in this Brand.", name]);

        RuleFor(p => p.Rate)
            .GreaterThanOrEqualTo(1);

        RuleFor(p => p.Image)
            .SetNonNullableValidator(imageValidator);

        RuleFor(g => g.BrandId)
            .NotEmpty()
            .MustAsync(async (id, ct) => await brandRepo.GetByIdAsync(id, ct) is not null)
                .WithMessage((_, id) => localizer["Brand with Id {0} does not exist.", id]);
    }
}

public class ProductByBrandAndNameSpec : SingleResultSpecification<Product>
{
    public ProductByBrandAndNameSpec(int brandId, string name) =>
        Query
            .Where(s => s.BrandId == brandId && s.Name == name);
}

public class CreateProductRequestHandler : IRequestHandler<CreateProductRequest, int>
{
    private readonly IRepositoryWithEvents<Product> _productRepo;
    private readonly IFileStorageService _fileStorage;
    public CreateProductRequestHandler(IRepositoryWithEvents<Product> productRepo, IFileStorageService fileStorage) =>
        (_productRepo, _fileStorage) = (productRepo, fileStorage);

    public async Task<int> Handle(CreateProductRequest req, CancellationToken ct)
    {
        string productImagePath = await _fileStorage.UploadAsync<Product>(req.Image, FileType.Image, ct);

        var product = new Product(req.Name, req.Description, req.Rate, productImagePath, req.BrandId);

        await _productRepo.AddAsync(product, ct);

        return product.Id;
    }
}