namespace Heroplate.Api.Domain.Catalog;

public class Brand : AuditableEntityWithName
{
    public Brand(string name, string? description)
        : base(name, description)
    {
    }

    public new void Update(string? name, string? description) =>
        base.Update(name, description);
}