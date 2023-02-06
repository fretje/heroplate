using System.ComponentModel.DataAnnotations;

namespace Heroplate.Api.Infrastructure.BackgroundJobs;

public class HangfireStorageSettings : IValidatableObject
{
    public string StorageProvider { get; set; } = default!;
    public string ConnectionString { get; set; } = default!;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrEmpty(StorageProvider))
        {
            yield return new ValidationResult(
                $"{nameof(HangfireStorageSettings)}.{nameof(StorageProvider)} is not configured",
                new[] { nameof(StorageProvider) });
        }

        if (string.IsNullOrEmpty(ConnectionString))
        {
            yield return new ValidationResult(
                $"{nameof(HangfireStorageSettings)}.{nameof(ConnectionString)} is not configured",
                new[] { nameof(ConnectionString) });
        }
    }
}