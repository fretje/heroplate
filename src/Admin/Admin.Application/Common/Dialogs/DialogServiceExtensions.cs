namespace Heroplate.Admin.Application.Common.Dialogs;

public static class DialogServiceExtensions
{
    private static readonly DialogOptions _optionsMedium = new() { CloseButton = true, MaxWidth = MaxWidth.Medium, FullWidth = true, CloseOnEscapeKey = true, Position = DialogPosition.TopCenter };
    private static readonly DialogOptions _optionsSmall = new() { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true, CloseOnEscapeKey = true, Position = DialogPosition.TopCenter };

    public static Task<DialogResult> ShowModalAsync<TDialog>(this IDialogService dialogService, DialogParameters parameters)
        where TDialog : ComponentBase =>
        dialogService.ShowModal<TDialog>(parameters).Result;

    public static IDialogReference ShowModal<TDialog>(this IDialogService dialogService, DialogParameters parameters)
        where TDialog : ComponentBase =>
        dialogService.Show<TDialog>("", parameters, _optionsMedium);

    public static void ConfirmAndLogout(this IDialogService dialogService) =>
        dialogService.Show<Logout>("", _optionsSmall);

    public static async Task<bool> ShowDeleteConfirmationAsync(this IDialogService dialogService, string contentText)
    {
        var parameters = new DialogParameters
        {
            { nameof(DeleteConfirmation.ContentText), contentText }
        };
        var dialog = await dialogService.ShowAsync<DeleteConfirmation>("Delete", parameters, _optionsSmall);
        var result = await dialog.Result;
        return !result.Canceled;
    }

    public static async Task<bool> ConfirmAndDeleteEntityAsync<TId>(this IDialogService dialogService, string entityTypeName, string? entityName, TId id, Func<TId, Task> deleteFunc, ISnackbar snackbar, IStringLocalizer localizer)
    {
        string name = entityName is null ? $"#{id}" : $"'{entityName}' (#{id})";
        return await dialogService.ShowDeleteConfirmationAsync(
            localizer["You're sure you want to delete {0} {1}?", localizer[entityTypeName], name])
            && await ApiHelper.ExecuteCallGuardedAsync(
                () => deleteFunc(id),
                snackbar,
                successMessage: $"{localizer[entityTypeName]} {name} {localizer["Deleted"]}");
    }

    public static Task<bool?> ShowMessageBoxAsync(this IDialogService dialogService, string title, string message, string? yesText = "OK", string? noText = null, string? cancelText = null, bool small = false) =>
        dialogService.ShowMessageBox(title, message, yesText, noText, cancelText, small ? _optionsSmall : _optionsMedium);
}