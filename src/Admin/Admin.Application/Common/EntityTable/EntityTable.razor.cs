using Mapster;

namespace Heroplate.Admin.Application.Common.EntityTable;

public partial class EntityTable<TEntity, TId, TRequest>
    where TId : notnull
    where TRequest : new()
{
    [Parameter]
    [EditorRequired]
    public EntityTableContext<TEntity, TId, TRequest> Context { get; set; } = default!;

    [Parameter]
    public bool Loading { get; set; }

    [Parameter]
    public string? SearchString { get; set; }
    [Parameter]
    public EventCallback<string> SearchStringChanged { get; set; }

    [Parameter]
    public RenderFragment? AdvancedSearchContent { get; set; }

    [Parameter]
    public RenderFragment<TEntity>? ActionsContent { get; set; }
    [Parameter]
    public RenderFragment<TEntity>? ExtraActions { get; set; }
    [Parameter]
    public RenderFragment? ExtraToolBarContent { get; set; }

    // this one is just forwarded to the table
    [Parameter]
    public RenderFragment<TEntity>? ChildRowContent { get; set; }

    // use this one if you want a built-in open/close button... also doesn't need the table with colspan
    [Parameter]
    public RenderFragment<TEntity>? DetailsContent { get; set; }

    // set this parameter (in conjunction with DetailsContent) to initially show the details for a specific entity
    [Parameter]
    public TId? ShowDetailsForId { get; set; }

    // set this one to true if you only want to allow one details pane to be open at the same time
    // (an already open one will be closed when opening another one)
    [Parameter]
    public bool ShowDetailsAllowOnlyOne { get; set; }

    [Parameter]
    public RenderFragment<TRequest>? EntityFields { get; set; }

    [Parameter]
    public int Elevation { get; set; } = 25;
    [Parameter]
    public Func<TEntity, int, string>? RowStyleFunc { get; set; }
    [Parameter]
    public EventCallback<TEntity> OnDetails { get; set; }

    [CascadingParameter]
    protected Task<AuthenticationState> AuthState { get; set; } = default!;

    private bool _canSearch;
    private bool _canCreate;
    private bool _canUpdate;
    private bool _canDelete;
    private bool _canExport;

    private bool _advancedSearchExpanded;

    private MudTable<TEntity> _table = default!;
    private IEnumerable<TEntity>? _entityList;
    private int _totalItems;

    public Task RefreshDataAsync() =>
        Context.IsClientContext
            ? LocalLoadDataAsync()
            : ServerLoadDataAsync();

    protected override async Task OnInitializedAsync()
    {
        if (ShowDetailsForId is not null)
        {
            _showDetailsForIds.Add(ShowDetailsForId);
        }

        var state = await AuthState;
        _canSearch = await CanDoAsync(Context.SearchPermission, state);
        _canCreate = await CanDoAsync(Context.CreatePermission, state);
        _canUpdate = await CanDoAsync(Context.UpdatePermission, state);
        _canDelete = await CanDoAsync(Context.DeletePermission, state);
        _canExport = await CanDoAsync(Context.ExportPermission, state);

        await LocalLoadDataAsync();
    }

    private async Task<bool> CanDoAsync(string? permission, AuthenticationState state) =>
        permission is not null &&
            (string.IsNullOrWhiteSpace(permission) ||
            await Authorizer.HasPermissionAsync(state.User, permission));

    private bool HasActions => _canUpdate || _canDelete || OnDetails.HasDelegate
        || DetailsContent is not null
        || (_canCreate && Context.GetDuplicateFunc is not null)
        || (Context.HasExtraActionsFunc is not null && Context.HasExtraActionsFunc());

    // Don't show Actions header when only the detailscontent arrow is shown
    private string ActionsHeader => _canUpdate || _canDelete || OnDetails.HasDelegate
        || (_canCreate && Context.GetDuplicateFunc is not null)
        || (Context.HasExtraActionsFunc is not null && Context.HasExtraActionsFunc())
            ? L["Actions"]
            : "";

    private bool CanUpdateEntity(TEntity entity) => _canUpdate && (Context.CanUpdateEntityFunc is null || Context.CanUpdateEntityFunc(entity));
    private bool CanDeleteEntity(TEntity entity) => _canDelete && (Context.CanDeleteEntityFunc is null || Context.CanDeleteEntityFunc(entity));

    // Client side paging/filtering
    private bool LocalSearch(TEntity entity) =>
        Context.ClientContext?.SearchFunc is { } searchFunc
            ? searchFunc(SearchString, entity)
            : string.IsNullOrWhiteSpace(SearchString);

    private async Task LocalLoadDataAsync()
    {
        if (Loading || Context.ClientContext is null)
        {
            return;
        }

        Loading = true;

        if (await ApiHelper.ExecuteCallGuardedAsync(
                () => Context.ClientContext.LoadDataFunc(), Snackbar)
            is List<TEntity> result)
        {
            _entityList = result;
        }

        Loading = false;
    }

    // Server Side paging/filtering

    private async Task OnSearchStringChangedAsync()
    {
        await SearchStringChanged.InvokeAsync(SearchString);
        await ServerLoadDataAsync();
    }

    private async Task ServerLoadDataAsync()
    {
        if (Context.IsServerContext)
        {
            await _table.ReloadServerData();
        }
    }

    private Func<TableState, Task<TableData<TEntity>>>? ServerReloadFunc =>
        Context.IsServerContext ? ServerReloadAsync : null;

    private async Task<TableData<TEntity>> ServerReloadAsync(TableState state)
    {
        if (!Loading && Context.ServerContext is not null)
        {
            Loading = true;
            StateHasChanged();

            var filter = new PaginationFilter { Keyword = SearchString };
            filter.FillWith(state);
            AddAdvancedSearch(filter);

            if (await ApiHelper.ExecuteCallGuardedAsync(
                    () => Context.ServerContext.SearchFunc(filter), Snackbar)
                is { } result)
            {
                _totalItems = result.TotalCount;
                _entityList = result.Data;
            }

            Loading = false;
            StateHasChanged();
        }

        return new TableData<TEntity> { TotalItems = _totalItems, Items = _entityList };
    }

    private void AddAdvancedSearch(BaseFilter filter)
    {
        if (!Context.AllColumnsChecked)
        {
            filter.AdvancedSearch = new()
            {
                Fields = Context.SearchFields,
                Keyword = filter.Keyword
            };
            filter.Keyword = null;
        }
    }

    private async Task InvokeModalAsync(TEntity? entity = default, TEntity? entityToDuplicate = default)
    {
        Loading = true;

        bool isCreate = entity is null;

        var parameters = new DialogParameters()
        {
            { nameof(AddEditModal<TRequest>.ChildContent), EntityFields },
            { nameof(AddEditModal<TRequest>.OnInitializedFunc), Context.EditFormInitializedFunc },
            { nameof(AddEditModal<TRequest>.IsCreate), isCreate }
        };

        Func<TRequest, Task> saveFunc;
        TRequest requestModel;
        string title, successMessage;

        if (isCreate)
        {
            saveFunc = Context.CreateFunc;

            requestModel =
                entityToDuplicate is not null && Context.GetDuplicateFunc is not null
                    ? await ApiHelper.ExecuteCallGuardedAsync(() => Context.GetDuplicateFunc(entityToDuplicate), Snackbar) is { } duplicateResult
                        ? duplicateResult
                        : new TRequest()
                    : Context.GetDefaultsFunc is not null
                            && await ApiHelper.ExecuteCallGuardedAsync(
                                    () => Context.GetDefaultsFunc(), Snackbar)
                                is { } defaultsResult
                        ? defaultsResult
                        : new TRequest();

            title = $"{L["Create a new {0}", L[Context.EntityTypeName]]}";
            successMessage = $"{Context.EntityTypeName} {L["Created"]}";
        }
        else
        {
            var id = Context.IdFunc(entity!);

            saveFunc = request => Context.UpdateFunc(id, request);

            requestModel =
                Context.GetDetailsFunc is not null
                    && await ApiHelper.ExecuteCallGuardedAsync(
                            () => Context.GetDetailsFunc(id),
                            Snackbar)
                        is { } detailsResult
                ? detailsResult
                : entity!.Adapt<TRequest>();

            if (entity is EntityWithNameDto entityWithName)
            {
                title = $"{L["Edit"]} {Context.EntityTypeName} '{entityWithName.Name}' (#{id})";
                successMessage = $"{Context.EntityTypeName} '{entityWithName.Name}' (#{id}) {L["Updated"]}";
            }
            else
            {
                title = $"{L["Edit"]} {Context.EntityTypeName} #{id}";
                successMessage = $"{Context.EntityTypeName} #{id} {L["Updated"]}";
            }
        }

        parameters.Add(nameof(AddEditModal<TRequest>.SaveFunc), saveFunc);
        parameters.Add(nameof(AddEditModal<TRequest>.RequestModel), requestModel);
        parameters.Add(nameof(AddEditModal<TRequest>.Title), title);
        parameters.Add(nameof(AddEditModal<TRequest>.SuccessMessage), successMessage);

        var dialog = DialogService.ShowModal<AddEditModal<TRequest>>(parameters);

        Context.SetAddEditModalRef(dialog);

        Loading = false;
        StateHasChanged();

        var result = await dialog.Result;

        if (!result.Canceled)
        {
            await RefreshDataAsync();
        }
    }

    private async Task DeleteAsync(TEntity entity)
    {
        if (await DialogService.ConfirmAndDeleteEntityAsync(
            Context.EntityTypeName,
            entity is EntityWithNameDto entityWithName ? entityWithName.Name : null,
            Context.IdFunc(entity),
            Context.DeleteFunc,
            Snackbar,
            L))
        {
            await RefreshDataAsync();
        }
    }

    private readonly HashSet<TId> _showDetailsForIds = new();

    private bool ShowDetailsFor(TEntity entity)
    {
        var id = Context.IdFunc(entity);
        return _showDetailsForIds.Contains(id);
    }

    private void SetShowDetailsFor(TEntity entity, bool show)
    {
        var id = Context.IdFunc(entity);
        if (show)
        {
            _showDetailsForIds.Add(id);
        }
        else
        {
            _showDetailsForIds.Remove(id);
        }
    }

    private int GetColumnCount()
    {
        int count = Context.Fields.Count;
        if (ActionsContent is not null || HasActions)
        {
            count++;
        }

        return count;
    }
}