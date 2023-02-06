namespace Heroplate.Api.Application.Common.Entities;

public class EntityWithNameRequest : IEntityWithNameRequest
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
}