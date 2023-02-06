using System.ComponentModel;
using System.Text.RegularExpressions;

namespace Heroplate.Api.Infrastructure.Common.Extensions;

public static partial class EnumExtensions
{
    public static string GetDescription(this Enum enumValue)
    {
        object[] attr = enumValue.GetType().GetField(enumValue.ToString())!
            .GetCustomAttributes(typeof(DescriptionAttribute), false);
        if (attr.Length > 0)
        {
            return ((DescriptionAttribute)attr[0]).Description;
        }

        string result = enumValue.ToString();
        result = LowerToUpperRegex().Replace(result, "$1 $2");
        result = LetterToNumberRegex().Replace(result, "$1 $2");
        result = NumberToLetterRegex().Replace(result, "$1 $2");
        result = WordStartRegex().Replace(result, " $1");
        return result;
    }

    public static List<string> GetDescriptionList(this Enum enumValue)
    {
        string result = enumValue.GetDescription();
        return result.Split(',').ToList();
    }

    [GeneratedRegex("([a-z])([A-Z])")]
    private static partial Regex LowerToUpperRegex();

    [GeneratedRegex("([A-Za-z])([0-9])")]
    private static partial Regex LetterToNumberRegex();

    [GeneratedRegex("([0-9])([A-Za-z])")]
    private static partial Regex NumberToLetterRegex();

    [GeneratedRegex("(?<!^)(?<! )([A-Z][a-z])")]
    private static partial Regex WordStartRegex();
}