namespace Heroplate.Api.Application.Common.Specification;

public class EntitiesByBaseFilterSpec<T> : Specification<T>
{
    public EntitiesByBaseFilterSpec(BaseFilter filter) =>
        Query.SearchBy(filter);
}

public class EntityDtosByBaseFilterSpec<T, TDto> : Specification<T, TDto>
{
    public EntityDtosByBaseFilterSpec(BaseFilter filter) =>
        Query.SearchBy(filter);
}