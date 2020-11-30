using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using SourceExpander.Expanded;

namespace SourceExpander.Test
{
    internal static class TestUtil
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
        public static IReadOnlyDictionary<string, SourceCode> GetExpandedFilesWithCore(Compilation compilation)
            => (IReadOnlyDictionary<string, SourceCode>)GetExpandedFiles(compilation);


        public static readonly MetadataReference[] DefaulMetadatas
            = GetDefaulMetadatas().ToArray();

        private static IEnumerable<MetadataReference> GetDefaulMetadatas()
        {
            var directory = Path.GetDirectoryName(typeof(object).Assembly.Location);
            foreach (var file in Directory.EnumerateFiles(directory, "System*.dll"))
            {
                yield return MetadataReference.CreateFromFile(file);
            }
            yield return MetadataReference.CreateFromFile(Assembly.Load("netstandard, Version=2.0.0.0").Location);
            yield return MetadataReference.CreateFromFile(typeof(SourceCode).GetTypeInfo().Assembly.Location);
            yield return MetadataReference.CreateFromFile(typeof(Expander).GetTypeInfo().Assembly.Location);
        }
    }
}
