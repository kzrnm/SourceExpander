using System.Text;

namespace SourceExpander
{
    public static class EnvironmentUtil
    {
        public static string JoinByStringBuilder(params string[] strs)
        {
            var sb = new StringBuilder();
            foreach (var s in strs)
                sb.AppendLine(s);
            return sb.ToString();
        }
        public static string ReplaceEOL(this string str) => str.Replace('\n', '\r');
    }
}
