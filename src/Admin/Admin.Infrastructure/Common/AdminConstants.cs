namespace Heroplate.Admin.Infrastructure.Common;

public static class AdminConstants
{
    public static readonly List<string> SupportedImageFormats = new()
    {
        ".jpeg",
        ".jpg",
        ".png"
    };
    public static readonly string StandardImageFormat = "image/jpeg";
    public static readonly int MaxImageWidth = 1500;
    public static readonly int MaxImageHeight = 1500;
    public static readonly long MaxAllowedSize = 1000000; // Allows Max File Size of 1 Mb.

    public static readonly string DefaultConnectionString = "Integrated Security=True;Trust Server Certificate=True";
}