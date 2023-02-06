namespace Heroplate.Api.Domain.Abstractions.Entities;

public interface IEntityWithName : IEntity<int>, IAggregateRoot
{
    string Name { get; }
    string? Description { get; }
}