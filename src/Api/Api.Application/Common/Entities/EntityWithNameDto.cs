namespace Heroplate.Api.Application.Common.Entities;

public class EntityWithNameDto
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
}