namespace Heroplate.Admin.Application.Common.EntityTable;

/// <summary>
/// Initialization Context for the EntityTable Component.
/// Use this one if you want to use Server Paging, Sorting and Filtering.
/// </summary>
/// <typeparam name="TEntity"></typeparam>
/// <typeparam name="TId"></typeparam>
/// <typeparam name="TRequest"></typeparam>
public class EntityServerTableContext<TEntity, TId, TRequest>
    : EntityTableContext<TEntity, TId, TRequest>
{
    /// <summary>
    /// A function that loads the specified page from the api with the specified search criteria
    /// and returns a PaginatedResult of TEntity.
    /// </summary>
    public Func<PaginationFilter, Task<PaginationResponse<TEntity>>> SearchFunc { get; }

    public bool EnableAdvancedSearch { get; }

    public EntityServerTableContext(
        List<EntityField<TEntity>> fields,
        Func<PaginationFilter, Task<PaginationResponse<TEntity>>> searchFunc,
        bool enableAdvancedSearch = false,
        Func<TEntity, TId>? idFunc = null,
        Func<Task<TRequest>>? getDefaultsFunc = null,
        Func<TRequest, Task>? createFunc = null,
        Func<TEntity, Task<TRequest>>? getDuplicateFunc = null,
        Func<TId, Task<TRequest>>? getDetailsFunc = null,
        Func<TId, TRequest, Task>? updateFunc = null,
        Func<TId, Task>? deleteFunc = null,
        string? entityTypeName = null,
        string? entityTypeNamePlural = null,
        string? searchPermission = null,
        string? createPermission = null,
        string? updatePermission = null,
        string? deletePermission = null,
        string? exportPermission = null,
        Func<TRequest, Task>? editFormInitializedFunc = null,
        Func<bool>? hasExtraActionsFunc = null,
        Func<TEntity, bool>? canUpdateEntityFunc = null,
        Func<TEntity, bool>? canDeleteEntityFunc = null)
        : base(
            fields,
            idFunc,
            getDefaultsFunc,
            createFunc,
            getDuplicateFunc,
            getDetailsFunc,
            updateFunc,
            deleteFunc,
            entityTypeName,
            entityTypeNamePlural,
            searchPermission,
            createPermission,
            updatePermission,
            deletePermission,
            exportPermission,
            editFormInitializedFunc,
            hasExtraActionsFunc,
            canUpdateEntityFunc,
            canDeleteEntityFunc)
    {
        SearchFunc = searchFunc;
        EnableAdvancedSearch = enableAdvancedSearch;
    }
}