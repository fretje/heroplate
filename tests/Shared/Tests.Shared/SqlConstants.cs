namespace Tests.Shared;

/// <summary>
/// Sql Constants.
/// </summary>
public class SqlConstants
{
    /// <summary>
    /// Name Escape Start.
    /// </summary>
    public string NameEscapeStart { get; } = "[";

    /// <summary>
    /// Name Escape End.
    /// </summary>
    public string NameEscapeEnd { get; } = "]";

    /// <summary>
    /// Escape a name (escape the chars and put it in brackets).
    /// </summary>
    /// <param name="name"></param>
    public string EscapedName(string name) => NameEscapeStart + EscapeChars(name) + NameEscapeEnd;

    /// <summary>
    /// Escape the individual chars of a name (use before enclosing it in brackets).
    /// </summary>
    /// <param name="name"></param>
    public static string? EscapeChars(string name) => name.Replace("]", "]]");
}