using System.Text.RegularExpressions;

namespace Heroplate.Api.Infrastructure.Common.Extensions;

public static partial class RegexExtensions
{
    public static string ReplaceWhitespace(this string input, string replacement) =>
        WhiteSpaceRegex().Replace(input, replacement);

    [GeneratedRegex("\\s+")]
    private static partial Regex WhiteSpaceRegex();
}