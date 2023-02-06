namespace Heroplate.Admin.Application.Common.EntityTable;

public partial class AddEditModal<TRequest> : IAddEditModal<TRequest>
{
    [Parameter]
    [EditorRequired]
    public RenderFragment<TRequest> ChildContent { get; set; } = default!;
    [Parameter]
    [EditorRequired]
    public TRequest RequestModel { get; set; } = default!;
    [Parameter]
    [EditorRequired]
    public Func<TRequest, Task> SaveFunc { get; set; } = default!;
    [Parameter]
    public Func<TRequest, Task>? OnInitializedFunc { get; set; }
    [Parameter]
    [EditorRequired]
    public string Title { get; set; } = default!;
    [Parameter]
    public bool IsCreate { get; set; }
    [Parameter]
    public string? SuccessMessage { get; set; }

    [CascadingParameter]
    private MudDialogInstance MudDialog { get; set; } = default!;

    private CustomValidation? _customValidation;

    public void ForceRender() => StateHasChanged();

    protected override Task OnInitializedAsync() =>
        OnInitializedFunc is not null
            ? OnInitializedFunc(RequestModel)
            : Task.CompletedTask;

    private async Task SaveAsync()
    {
        if (await ApiHelper.ExecuteCallGuardedAsync(
            () => SaveFunc(RequestModel), Snackbar, _customValidation, SuccessMessage))
        {
            MudDialog.Close();
        }
    }
}