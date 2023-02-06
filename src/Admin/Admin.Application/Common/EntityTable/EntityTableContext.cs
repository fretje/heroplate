using System.Diagnostics.CodeAnalysis;

namespace Heroplate.Admin.Application.Common.EntityTable;

/// <summary>
/// Abstract base class for the initialization Context of the EntityTable Component.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <typeparam name="TId">The type of the id of the entity.</typeparam>
/// <typeparam name="TRequest">The type of the Request which is used on the AddEditModal and which is sent with the CreateFunc and UpdateFunc.</typeparam>
public abstract class EntityTableContext<TEntity, TId, TRequest>
{
    /// <summary>
    /// The columns you want to display on the table.
    /// </summary>
    public List<EntityField<TEntity>> Fields { get; }

    private readonly Func<TEntity, TId>? _idFunc;

    /// <summary>
    /// A function that returns the Id of the entity. This is only needed when using the CRUD functionality.
    /// </summary>
    [NotNull]
    public Func<TEntity, TId>? IdFunc
    {
        get => _idFunc ?? throw new InvalidOperationException($"{nameof(IdFunc)} not set.");
        init => _idFunc = value;
    }

    /// <summary>
    /// A function that executes the GetDefaults method on the api (or supplies defaults locally) and returns
    /// a Task of Result of TRequest. When not supplied, a TRequest is simply newed up.
    /// No need to check for error messages or api exceptions. These are automatically handled by the component.
    /// </summary>
    public Func<Task<TRequest>>? GetDefaultsFunc { get; }

    private readonly Func<TRequest, Task>? _createFunc;

    /// <summary>
    /// A function that executes the Create method on the api with the supplied entity and returns a Task of Result.
    /// No need to check for error messages or api exceptions. These are automatically handled by the component.
    /// </summary>
    [NotNull]
    public Func<TRequest, Task>? CreateFunc
    {
        get => _createFunc ?? throw new InvalidOperationException($"{nameof(CreateFunc)} not set.");
        init => _createFunc = value;
    }

    /// <summary>
    /// A function that duplicates a given entity into a request to create a new one.
    /// </summary>
    public Func<TEntity, Task<TRequest>>? GetDuplicateFunc { get; }

    /// <summary>
    /// A function that executes the GetDetails method on the api with the supplied Id and returns a Task of Result of TRequest.
    /// No need to check for error messages or api exceptions. These are automatically handled by the component.
    /// When not supplied, the TEntity out of the _entityList is supplied using the IdFunc and converted using mapster.
    /// </summary>
    public Func<TId, Task<TRequest>>? GetDetailsFunc { get; }

    private readonly Func<TId, TRequest, Task>? _updateFunc;

    /// <summary>
    /// A function that executes the Update method on the api with the supplied entity and returns a Task of Result.
    /// When not supplied, the TEntity from the list is mapped to TCreateRequest using mapster.
    /// No need to check for error messages or api exceptions. These are automatically handled by the component.
    /// </summary>
    [NotNull]
    public Func<TId, TRequest, Task>? UpdateFunc
    {
        get => _updateFunc ?? throw new InvalidOperationException($"{nameof(UpdateFunc)} not set.");
        init => _updateFunc = value;
    }

    private readonly Func<TId, Task>? _deleteFunc;

    /// <summary>
    /// A function that executes the Delete method on the api with the supplied entity id and returns a Task of Result.
    /// No need to check for error messages or api exceptions. These are automatically handled by the component.
    /// </summary>
    [NotNull]
    public Func<TId, Task>? DeleteFunc
    {
        get => _deleteFunc ?? throw new InvalidOperationException($"{nameof(DeleteFunc)} not set.");
        init => _deleteFunc = value;
    }

    private readonly string? _entityTypeName;

    /// <summary>
    /// The name of the entity. This is used in the title of the add/edit modal and delete confirmation.
    /// </summary>
    [NotNull]
    public string? EntityTypeName
    {
        get => _entityTypeName ?? throw new InvalidOperationException($"{nameof(EntityTypeName)} not set.");
        init => _entityTypeName = value;
    }

    private readonly string? _entityTypeNamePlural;

    /// <summary>
    /// The plural name of the entity. This is used in the "Search for ..." placeholder.
    /// </summary>
    [NotNull]
    public string? EntityTypeNamePlural
    {
        get => _entityTypeNamePlural ?? throw new InvalidOperationException($"{nameof(EntityTypeNamePlural)} not set.");
        init => _entityTypeNamePlural = value;
    }

    /// <summary>
    /// The permission required to search. This is empty by default.
    /// When null, no search functionality will be available.
    /// When an empty string, search funtionality will be enabled,
    /// otherwise it will only be enabled if the user has this permission.
    /// </summary>
    public string? SearchPermission { get; }

    /// <summary>
    /// The permission name of the create permission. This is empty by default.
    /// When null, no create functionality will be available.
    /// When an empty string, create funtionality will be enabled,
    /// otherwise it will only be enabled if the user has this permission.
    /// </summary>
    public string? CreatePermission { get; }

    /// <summary>
    /// The permission name of the update permission. This is null by default.
    /// When null, no update functionality will be available.
    /// When an empty string is, update funtionality will be enabled,
    /// otherwise it will only be enabled if the user has this permission.
    /// </summary>
    public string? UpdatePermission { get; }

    /// <summary>
    /// The permission name of the delete permission. This is null by default.
    /// When null, no delete functionality will be available.
    /// When an empty string, delete funtionality will be enabled,
    /// otherwise it will only be enabled if the user has this permission.
    /// </summary>
    public string? DeletePermission { get; }

    /// <summary>
    /// The permission name of the export permission. This is null by default.
    /// When null, no export functionality will be available.
    /// When an empty string, export funtionality will be enabled,
    /// otherwise it will only be enabled if the user has this permission.
    /// </summary>
    public string? ExportPermission { get; }

    /// <summary>
    /// Use this if you want to run initialization during OnInitialized of the AddEdit form.
    /// </summary>
    public Func<TRequest, Task>? EditFormInitializedFunc { get; }

    /// <summary>
    /// Use this if you want to check for permissions of content in the ExtraActions RenderFragment.
    /// The extra actions won't be available when this returns false.
    /// </summary>
    public Func<bool>? HasExtraActionsFunc { get; set; }

    /// <summary>
    /// Use this if you want to disable the update functionality for specific entities in the table.
    /// </summary>
    public Func<TEntity, bool>? CanUpdateEntityFunc { get; set; }

    /// <summary>
    /// Use this if you want to disable the delete functionality for specific entities in the table.
    /// </summary>
    public Func<TEntity, bool>? CanDeleteEntityFunc { get; set; }

    protected EntityTableContext(
        List<EntityField<TEntity>> fields,
        Func<TEntity, TId>? idFunc,
        Func<Task<TRequest>>? getDefaultsFunc,
        Func<TRequest, Task>? createFunc,
        Func<TEntity, Task<TRequest>>? getDuplicateFunc,
        Func<TId, Task<TRequest>>? getDetailsFunc,
        Func<TId, TRequest, Task>? updateFunc,
        Func<TId, Task>? deleteFunc,
        string? entityTypeName,
        string? entityTypeNamePlural,
        string? searchPermission,
        string? createPermission,
        string? updatePermission,
        string? deletePermission,
        string? exportPermission,
        Func<TRequest, Task>? editFormInitializedFunc,
        Func<bool>? hasExtraActionsFunc,
        Func<TEntity, bool>? canUpdateEntityFunc,
        Func<TEntity, bool>? canDeleteEntityFunc)
    {
        Fields = fields;
        EntityTypeName = entityTypeName;
        EntityTypeNamePlural = entityTypeNamePlural;
        IdFunc = idFunc;
        GetDefaultsFunc = getDefaultsFunc;
        CreateFunc = createFunc;
        GetDuplicateFunc = getDuplicateFunc;
        GetDetailsFunc = getDetailsFunc;
        UpdateFunc = updateFunc;
        DeleteFunc = deleteFunc;
        SearchPermission = searchPermission ?? "";
        CreatePermission = createPermission;
        UpdatePermission = updatePermission;
        DeletePermission = deletePermission;
        ExportPermission = exportPermission;
        EditFormInitializedFunc = editFormInitializedFunc;
        HasExtraActionsFunc = hasExtraActionsFunc;
        CanUpdateEntityFunc = canUpdateEntityFunc;
        CanDeleteEntityFunc = canDeleteEntityFunc;
    }

    // AddEdit modal
    private IDialogReference? _addEditModalRef;

    internal void SetAddEditModalRef(IDialogReference dialog) =>
        _addEditModalRef = dialog;

    public IAddEditModal<TRequest> AddEditModal =>
        _addEditModalRef?.Dialog as IAddEditModal<TRequest>
        ?? throw new InvalidOperationException("AddEditModal is only available when the modal is shown.");

    // Shortcuts
    public EntityClientTableContext<TEntity, TId, TRequest>? ClientContext => this as EntityClientTableContext<TEntity, TId, TRequest>;
    public EntityServerTableContext<TEntity, TId, TRequest>? ServerContext => this as EntityServerTableContext<TEntity, TId, TRequest>;
    public bool IsClientContext => ClientContext is not null;
    public bool IsServerContext => ServerContext is not null;

    // Advanced Search
    public bool AllColumnsChecked =>
        Fields.All(f => f.CheckedForSearch);
    public void AllColumnsCheckChanged(bool checkAll) =>
        Fields.ForEach(f => f.CheckedForSearch = checkAll);
    public bool AdvancedSearchEnabled =>
        ServerContext?.EnableAdvancedSearch is true;
    public List<string> SearchFields =>
        Fields.Where(f => f.CheckedForSearch).Select(f => f.SortLabel).ToList();
}