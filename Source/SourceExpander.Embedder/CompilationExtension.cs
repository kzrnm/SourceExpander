using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace SourceExpander
{
    public static class CompilationExtension
    {
        public static bool HasType(this Compilation compilation, string typeFullName)
            => compilation.GetTypeByMetadataName(typeFullName) != null;

        public static string ResolveCommomPrefix(this Compilation compilation)
        {
            return ResolveCommomPrefix(compilation.SyntaxTrees.Select(tree => tree.FilePath));
        }
        public static string ResolveCommomPrefix(IEnumerable<string> strs)
        {
            var sorted = new SortedSet<string>(strs, StringComparer.Ordinal);
            if (sorted.Count < 1) return "";
            if (sorted.Count < 2)
            {
                var p = sorted.Min;
                var name = Path.GetFileName(p);
                if (!p.EndsWith(name))
                    return "";

                return p.Substring(0, p.Length - name.Length);
            }

            var min = sorted.Min;
            var max = sorted.Max;

            for (int i = 0; i < min.Length && i < max.Length; i++)
            {
                if (min[i] != max[i])
                    return min.Substring(0, i);
            }
            return min;
        }
    }
}
