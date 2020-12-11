using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;

namespace SourceExpander
{
    internal static class GeneratorUtil
    {
        public static object GetExpandedFiles(Compilation compilation)
        {
            using var ms = new MemoryStream();
            if (!compilation.Emit(ms).Success)
                throw new ArgumentException("compilation is failed", nameof(compilation));
            ms.Position = 0;
            var alc = new AssemblyLoadContext("GetExpandedFiles", true);
            try
            {
                return alc.LoadFromStream(ms)
                    .GetType("SourceExpander.Expanded.ExpandedContainer")
                    .GetProperty("Files").GetValue(null);
            }
            finally
            {
                alc.Unload();
            }
        }

        public static IEnumerable<MetadataReference> GetDefaulMetadatas()
        {
            var directory = Path.GetDirectoryName(typeof(object).Assembly.Location);
            foreach (var file in Directory.EnumerateFiles(directory, "System*.dll"))
            {
                yield return MetadataReference.CreateFromFile(file);
            }
        }
    }
}
