namespace Heroplate.Admin.Application.Common;

public static class ApiHelper
{
    public static async Task<T?> ExecuteCallGuardedAsync<T>(
        Func<Task<T>> call,
        ISnackbar snackbar,
        CustomValidation? customValidation = null,
        string? successMessage = null)
    {
        customValidation?.ClearErrors();

        (bool success, var result, string? error, var validationErrors) =
            await TryExecuteCallGuardedAsync(call);

        if (success)
        {
            if (!string.IsNullOrWhiteSpace(successMessage))
            {
                snackbar.Add(successMessage, Severity.Success);
            }

            return result;
        }
        else if (validationErrors is not null)
        {
            customValidation?.DisplayErrors(validationErrors);
        }
        else if (!string.IsNullOrWhiteSpace(error))
        {
            snackbar.Add(error, Severity.Error);
        }

        return default;
    }

    public static Task<bool> ExecuteCallGuardedAsync(
        Func<Task> call,
        ISnackbar snackbar,
        CustomValidation? customValidation = null,
        string? successMessage = null)
    {
        return ExecuteCallGuardedAsync(
            async () =>
            {
                await call();
                return true;
            },
            snackbar,
            customValidation,
            successMessage);
    }

    public static async Task<(bool Success, T? Result, string? Error, IDictionary<string, ICollection<string>>? ValidationErrors)> TryExecuteCallGuardedAsync<T>(
        Func<Task<T>> call)
    {
        bool success = false;
        T? result = default;
        string? errorMessage = default;
        IDictionary<string, ICollection<string>>? validationErrors = default;
        try
        {
            result = await call();
            success = true;
        }
        catch (ApiException<HttpValidationProblemDetails> ex)
        {
            errorMessage = "Validation errors occurred.";

            if (ex.Result.Errors is { } errors)
            {
                validationErrors = errors;
            }
        }
        catch (ApiException<ProblemDetails> ex)
        {
            errorMessage = ex.Result.Detail ?? ex.Result.Title;
        }

        return (success, result, errorMessage, validationErrors);
    }
}