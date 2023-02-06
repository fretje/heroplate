namespace Heroplate.Api.Application.Common.Entities;

public class EntityDtoByIdSpec<TEntity, TEntityDto> : SingleResultSpecification<TEntity, TEntityDto>
    where TEntity : Domain.Abstractions.Entities.IEntity<int>
{
    public EntityDtoByIdSpec(int id) =>
        Query.Where(h => h.Id == id);
}