namespace Heroplate.Api.Application.Common.Specification;

public class EntitiesByPaginationFilterSpec<T> : EntitiesByBaseFilterSpec<T>
{
    public EntitiesByPaginationFilterSpec(PaginationFilter filter)
        : base(filter) =>
        Query.PaginateBy(filter);
}

public class EntityDtosByPaginationFilterSpec<T, TDto> : EntityDtosByBaseFilterSpec<T, TDto>
{
    public EntityDtosByPaginationFilterSpec(PaginationFilter filter)
        : base(filter) =>
        Query.PaginateBy(filter);
}