using System;
using System.IO;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;

namespace SourceExpander.Embedder
{
    public class EmbeddingGeneratorTestBase : GeneratorTestBase
    {
        public static readonly MetadataReference expanderCoreReference = MetadataReference.CreateFromFile(typeof(SourceFileInfo).Assembly.Location);

        internal static object GetExpandedFiles(Compilation compilation)
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
    }
}
