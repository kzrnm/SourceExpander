using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace SourceExpander
{
    public class GeneratorTestBase
    {
        public static CSharpCompilation CreateCompilation(
            IEnumerable<SyntaxTree> syntaxTrees,
            CSharpCompilationOptions compilationOptions,
            IEnumerable<MetadataReference> additionalMetadatas = null,
            string assemblyName = "TestAssembly")
        {
            additionalMetadatas ??= Array.Empty<MetadataReference>();
            return CSharpCompilation.Create(
                assemblyName: assemblyName,
                syntaxTrees: syntaxTrees,
                references: DefaultMetadatas.Concat(additionalMetadatas),
                options: compilationOptions);
        }

        private static IEnumerable<MetadataReference> DefaultMetadatas { get; } = GetDefaultMetadatas();
        private static IEnumerable<MetadataReference> GetDefaultMetadatas()
        {
            var directory = Path.GetDirectoryName(typeof(object).Assembly.Location);
            foreach (var file in Directory.EnumerateFiles(directory, "System*.dll"))
            {
                yield return MetadataReference.CreateFromFile(file);
            }
        }
    }
}
