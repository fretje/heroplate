using Heroplate.Api.Domain.Catalog;

namespace Heroplate.Api.Application.Catalog.Brands;

public class UpdateBrandRequest : CreateBrandRequest, IUpdateEntityRequest
{
    public int Id { get; set; }
}

public class UpdateBrandRequestValidator : AbstractValidator<UpdateBrandRequest>
{
    public UpdateBrandRequestValidator(IReadRepository<Brand> brandRepo, IStringLocalizer<CreateBrandRequestValidator> localizer) =>
        Include(new CreateBrandRequestValidator(brandRepo, localizer));
}

public class UpdateBrandRequestHandler : IRequestHandler<UpdateBrandRequest, int>
{
    private readonly IRepositoryWithEvents<Brand> _brandRepo;
    private readonly IStringLocalizer _localizer;
    public UpdateBrandRequestHandler(IRepositoryWithEvents<Brand> brandRepo, IStringLocalizer<UpdateBrandRequestHandler> localizer) =>
        (_brandRepo, _localizer) = (brandRepo, localizer);

    public async Task<int> Handle(UpdateBrandRequest req, CancellationToken ct)
    {
        var brand = await _brandRepo.GetByIdAsync(req.Id, ct)
            ?? throw new NotFoundException(_localizer["Brand with Id {0} not found.", req.Id]);

        brand.Update(req.Name, req.Description);

        await _brandRepo.UpdateAsync(brand, ct);

        return req.Id;
    }
}