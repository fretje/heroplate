using Heroplate.Api.Domain.Catalog;

namespace Heroplate.Api.Application.Catalog.Brands;

public class CreateBrandRequest : EntityWithNameRequest
{
}

public class CreateBrandRequestValidator : AbstractValidator<CreateBrandRequest>
{
    public CreateBrandRequestValidator(IReadRepository<Brand> brandRepo, IStringLocalizer<CreateBrandRequestValidator> localizer) =>
        Include(new EntityWithNameRequestValidator<Brand>(brandRepo, localizer));
}

public class CreateBrandRequestHandler : IRequestHandler<CreateBrandRequest, int>
{
    private readonly IRepositoryWithEvents<Brand> _brandRepo;
    public CreateBrandRequestHandler(IRepositoryWithEvents<Brand> brandRepo) => _brandRepo = brandRepo;

    public async Task<int> Handle(CreateBrandRequest req, CancellationToken ct)
    {
        var brand = new Brand(req.Name, req.Description);

        await _brandRepo.AddAsync(brand, ct);

        return brand.Id;
    }
}