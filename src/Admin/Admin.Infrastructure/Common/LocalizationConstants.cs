namespace Heroplate.Admin.Infrastructure.Common;

public static class LocalizationConstants
{
    public static readonly LanguageCode[] SupportedLanguages =
    {
        new("en-BE", "English - Belgium"),
    };
}

public record LanguageCode(string Code, string DisplayName, bool IsRTL = false);