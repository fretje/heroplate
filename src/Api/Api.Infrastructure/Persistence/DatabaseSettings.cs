using System.ComponentModel.DataAnnotations;

namespace Heroplate.Api.Infrastructure.Persistence;

public class DatabaseSettings : IValidatableObject
{
    public string DBProvider { get; set; } = default!;
    public string ConnectionString { get; set; } = default!;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrEmpty(DBProvider))
        {
            yield return new ValidationResult(
                $"{nameof(DatabaseSettings)}.{nameof(DBProvider)} is not configured",
                new[] { nameof(DBProvider) });
        }

        if (string.IsNullOrEmpty(ConnectionString))
        {
            yield return new ValidationResult(
                $"{nameof(DatabaseSettings)}.{nameof(ConnectionString)} is not configured",
                new[] { nameof(ConnectionString) });
        }
    }
}