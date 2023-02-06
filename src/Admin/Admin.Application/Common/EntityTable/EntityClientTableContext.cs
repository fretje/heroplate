namespace Heroplate.Admin.Application.Common.EntityTable;

/// <summary>
/// Initialization Context for the EntityTable Component.
/// Use this one if you want to use Client Paging, Sorting and Filtering.
/// </summary>
/// <typeparam name="TEntity"></typeparam>
/// <typeparam name="TId"></typeparam>
/// <typeparam name="TRequest"></typeparam>
public class EntityClientTableContext<TEntity, TId, TRequest>
    : EntityTableContext<TEntity, TId, TRequest>
{
    /// <summary>
    /// A function that loads all the data for the table from the api and returns a ListResult of TEntity.
    /// </summary>
    public Func<Task<List<TEntity>?>> LoadDataFunc { get; }

    /// <summary>
    /// A function that returns a boolean which indicates whether the supplied entity meets the search criteria
    /// (the supplied string is the search string entered).
    /// </summary>
    public Func<string?, TEntity, bool> SearchFunc { get; }

    public EntityClientTableContext(
        List<EntityField<TEntity>> fields,
        Func<Task<List<TEntity>?>> loadDataFunc,
        Func<string?, TEntity, bool> searchFunc,
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
        LoadDataFunc = loadDataFunc;
        SearchFunc = searchFunc;
    }
}