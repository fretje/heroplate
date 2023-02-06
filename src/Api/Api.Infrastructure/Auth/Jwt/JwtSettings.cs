using System.ComponentModel.DataAnnotations;

namespace Heroplate.Api.Infrastructure.Auth.Jwt;

public class JwtSettings : IValidatableObject
{
    public string Key { get; set; } = default!;

    public int TokenExpirationInMinutes { get; set; }

    public int RefreshTokenExpirationInDays { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrEmpty(Key))
        {
            yield return new ValidationResult("No Key defined in JwtSettings config", new[] { nameof(Key) });
        }
    }
}