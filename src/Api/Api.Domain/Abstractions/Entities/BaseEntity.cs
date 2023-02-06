using System.ComponentModel.DataAnnotations.Schema;
using MassTransit;

namespace Heroplate.Api.Domain.Abstractions.Entities;

public abstract class BaseEntity : IEntity
{
    [NotMapped]
    public List<DomainEvent> DomainEvents { get; } = new();
}

public abstract class BaseEntity<TId> : BaseEntity, IEntity<TId>
{
    public TId Id { get; init; } = default!;

    protected BaseEntity()
    {
        if (typeof(TId) == typeof(Guid))
        {
            Id = (TId)(object)NewId.Next().ToGuid();
        }
    }
}