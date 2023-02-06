using System.Diagnostics;
using System.Reflection;

namespace Heroplate.Api.Infrastructure.Common;

public static class VersionInfo
{
    private static Version? _version;
    public static Version Version => _version ??= Version.Parse(ProductVersion.Split('+')[0]);

    private static string? _productVersion;
    public static string ProductVersion
    {
        get
        {
            if (_productVersion is null)
            {
                string? assemblyLocation = Assembly.GetExecutingAssembly().Location;
                if (string.IsNullOrWhiteSpace(assemblyLocation))
                {
                    // happens when publishing as single file
                    assemblyLocation = Environment.ProcessPath;
                }

                _productVersion =
                    !string.IsNullOrWhiteSpace(assemblyLocation)
                        && FileVersionInfo.GetVersionInfo(assemblyLocation).ProductVersion is { } version
                        && !string.IsNullOrWhiteSpace(version)
                            ? version
                            : "N/A";
            }

            return _productVersion;
        }
    }

    private static string? _gitSha;
    public static string GitSha
    {
        get
        {
            if (_gitSha is null)
            {
                string? productVersion = ProductVersion;
                int pos = productVersion.IndexOf('+') + 1;
                _gitSha = pos > 0 && pos < productVersion.Length
                    ? productVersion[pos..]
                    : "N/A";
            }

            return _gitSha;
        }
    }

    public static string GitShaShort => GitSha.Length > 6 ? GitSha[..6] : GitSha;
}