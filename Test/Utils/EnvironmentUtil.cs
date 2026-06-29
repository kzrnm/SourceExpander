using System.Text;

namespace SourceExpander;

public static class EnvironmentUtil
{
    public static string JoinByStringBuilder(params IEnumerable<string> strs)
    {
        var sb = new StringBuilder();
        foreach (var s in strs)
            sb.AppendLine(s);
        return sb.ToString();
    }
    public static string ReplaceEOL(this string str)
        => JoinByStringBuilder(str.Split(["\r\n", "\n"], StringSplitOptions.None));
}
