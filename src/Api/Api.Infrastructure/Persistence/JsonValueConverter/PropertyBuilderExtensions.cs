using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Heroplate.Api.Infrastructure.Persistence.JsonValueConverter;

public static class PropertyBuilderExtensions
{
    /// <summary>
    /// Serializes the field as a Json string in database.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="propertyBuilder"></param>
    public static PropertyBuilder<T> HasJsonValueConversion<T>(this PropertyBuilder<T> propertyBuilder)
        where T : class =>
        propertyBuilder
            .HasConversion(
                new JsonValueConverter<T>(),
                new JsonValueComparer<T>());
}