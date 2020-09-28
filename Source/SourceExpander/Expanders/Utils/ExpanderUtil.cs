using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SourceExpander.Expanders.Utils
{
    internal static class ExpanderUtil
    {
        public static string[] SortedUsings(IEnumerable<string> usings)
        {
            var arr = usings.ToArray();
            Array.Sort(arr, (a, b) => StringComparer.Ordinal.Compare(a.TrimEnd(';'), b.TrimEnd(';')));
            return arr;
        }
        public static IEnumerable<string> ToLines(string? str)
        {
            if (str == null) yield break;
            using var sr = new StringReader(str);
            for (var line = sr.ReadLine(); line != null; line = sr.ReadLine())
                yield return line;
        }

        public static string ToSimpleClassName(string className)
        {
            int l, r;
            // AtCoder.INumOperator<T> → INumOperator<T>
            for (l = className.Length - 1; l >= 0; l--)
                if (className[l] == '.')
                    break;
            ++l;


            // INumOperator<T> → INumOperator
            for (r = l; r < className.Length; r++)
                if (className[r] == '<')
                    break;

            return className.Substring(l, r - l);
        }


        private static readonly Regex usingRegex = new Regex(@"using\s+(.+);", RegexOptions.Compiled);
        public static string? ParseNamespace(string usingDirective)
        {
            var g = usingRegex.Match(usingDirective).Groups;
            if (g.Count < 2) return null;
            return g[1].Value;
        }
    }
}
