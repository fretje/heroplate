namespace Heroplate.Api.Domain.Abstractions.Entities;

public abstract class AuditableEntityWithName : AuditableEntity<int>, IEntityWithName
{
    public string Name { get; protected set; } = default!;
    public string? Description { get; protected set; }

    protected AuditableEntityWithName(string name, string? description)
    {
        Name = name;
        Description = description;
    }

    protected void Update(string? name, string? description)
    {
        if (name is not null && Name?.Equals(name, StringComparison.Ordinal) is not true)
        {
            Name = name;
        }

        if (description is not null && Description?.Equals(description, StringComparison.Ordinal) is not true)
        {
            Description = description;
        }
    }
}