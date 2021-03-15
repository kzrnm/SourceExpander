using System;

namespace SourceExpander
{
    public static class EnvironmentExtension
    {
        public static string ReplaceEOL(this string str)
        {
            if (OperatingSystem.IsWindows())
                return str.Replace("\r\n", "\n").Replace("\n", "\r\n")
                    .Replace("\\r\\n", "\\n").Replace("\\n", "\\r\\n");

            return str.Replace("\r\n", "\n")
                .Replace("\\r\\n", "\\n");
        }
    }
}
