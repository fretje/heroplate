using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Heroplate.Api.Infrastructure.Persistence.JsonValueConverter;

/// <summary>
/// Converts complex field to/from JSON string.
/// </summary>
/// <typeparam name="T">Model field type.</typeparam>
/// <remarks>See more: https://docs.microsoft.com/en-us/ef/core/modeling/value-conversions.</remarks>
internal class JsonValueConverter<T> : ValueConverter<T, string>
    where T : class
{
    public JsonValueConverter(ConverterMappingHints? mappingHints = null)
        : base(
            v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
            v => JsonSerializer.Deserialize<T>(v, (JsonSerializerOptions?)null)!,
            mappingHints)
    {
    }
}