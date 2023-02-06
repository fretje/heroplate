namespace Heroplate.Api.Domain.Catalog;

public class Product : AuditableEntityWithName
{
    public decimal Rate { get; private set; }
    public string? ImagePath { get; private set; }
    public int BrandId { get; private set; }
    public virtual Brand Brand { get; private set; } = default!;

    public Product(string name, string? description, decimal rate, string? imagePath, int brandId)
        : base(name, description)
    {
        Rate = rate;
        ImagePath = imagePath;
        BrandId = brandId;
    }

    public void Update(string? name, string? description, decimal? rate, string? imagePath, int? brandId)
    {
        if (name is not null && Name?.Equals(name) is not true)
        {
            Name = name;
        }

        if (description is not null && Description?.Equals(description) is not true)
        {
            Description = description;
        }

        if (rate.HasValue && Rate != rate)
        {
            Rate = rate.Value;
        }

        if (brandId.HasValue && !BrandId.Equals(brandId.Value))
        {
            BrandId = brandId.Value;
        }

        if (imagePath is not null && ImagePath?.Equals(imagePath) is not true)
        {
            ImagePath = imagePath;
        }
    }

    public void ClearImagePath() => ImagePath = string.Empty;
}