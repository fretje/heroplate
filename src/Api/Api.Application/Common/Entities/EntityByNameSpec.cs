using Heroplate.Api.Domain.Abstractions.Entities;

namespace Heroplate.Api.Application.Common.Entities;

public class EntityByNameSpec<TEntity> : SingleResultSpecification<TEntity>
    where TEntity : class, IEntityWithName
{
    public EntityByNameSpec(string name) =>
        Query
            .Where(s => s.Name == name);
}