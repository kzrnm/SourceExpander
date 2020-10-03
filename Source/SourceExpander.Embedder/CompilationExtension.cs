using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace SourceExpander
{
    public static class CompilationExtension
    {
        public static bool HasType(this Compilation compilation, string typeFullName)
            => compilation.GetTypeByMetadataName(typeFullName) != null;

        private static readonly Dictionary<object, string> _commomPrefixCache = new Dictionary<object, string>();
        public static string ResolveCommomPrefix(this Compilation compilation)
        {
            if (_commomPrefixCache.TryGetValue(compilation, out var val)) return val;
            return _commomPrefixCache[compilation] = ResolveCommomPrefix(compilation.SyntaxTrees.Select(tree => tree.FilePath));
        }
        public static string ResolveCommomPrefix(IEnumerable<string> strs)
        {
            var sorted = new SortedSet<string>(strs, StringComparer.Ordinal);
            if (sorted.Count < 2) return "";
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
