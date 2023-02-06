using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Heroplate.Api.Infrastructure.Persistence.JsonValueConverter;

/// <summary>
/// Compares two objects.
/// Required to make EF Core change tracking work for complex value converted objects.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <remarks>
/// For objects that implement <see cref="ICloneable"/> and <see cref="IEquatable{T}"/>,
/// those implementations will be used for cloning and equality.
/// For plain objects, fall back to deep equality comparison using JSON serialization
/// (safe, but inefficient).
/// </remarks>
internal class JsonValueComparer<T> : ValueComparer<T>
{
    public JsonValueComparer()
        : base(
            (left, right) => DoEquals(left, right),
            instance => DoGetHashCode(instance),
            instance => DoGetSnapshot(instance))
    {
    }

    private static bool DoEquals(T? left, T? right) =>
        left is IEquatable<T> equatable
            ? equatable.Equals(right)
            : Json(left).Equals(Json(right), StringComparison.Ordinal);

    private static int DoGetHashCode(T instance) =>
        instance is IEquatable<T>
            ? instance.GetHashCode()
            : Json(instance).GetHashCode();

    private static T DoGetSnapshot(T instance) =>
        instance is ICloneable cloneable
            ? (T)cloneable.Clone()
            : JsonSerializer.Deserialize<T>(Json(instance))!;

    private static string Json(T? instance) =>
        JsonSerializer.Serialize(instance);
}