using System.Reflection;

namespace Heroplate.Api.Infrastructure.Common.Extensions;

public static class AssemblyExtensions
{
    public static string GetManifestResourceText(this Assembly assembly, string resourceName)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        Stream? stream = null;
        try
        {
            stream = assembly.GetManifestResourceStream(resourceName)
                ?? throw new InvalidOperationException(
                    $"Unable to get stream for embedded resource '{resourceName}' in assembly '{assembly.FullName}'.");

            using var reader = new StreamReader(stream);
            stream = null;
            return reader.ReadToEnd();
        }
        finally
        {
            stream?.Dispose();
        }
    }
}