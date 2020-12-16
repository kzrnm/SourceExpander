using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SourceExpander
{
    public partial class GeneratorTestBase
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

        protected static GeneratorResult RunGenerator(
               Compilation compilation,
               ISourceGenerator generator,
               IEnumerable<AdditionalText> additionalTexts = null,
               CSharpParseOptions parseOptions = null,
               AnalyzerConfigOptionsProvider optionsProvider = null)
            => RunGenerator(compilation, new[] { generator }, additionalTexts, parseOptions, optionsProvider);
        protected static GeneratorResult RunGenerator(
               Compilation compilation,
               IEnumerable<ISourceGenerator> generators,
               IEnumerable<AdditionalText> additionalTexts = null,
               CSharpParseOptions parseOptions = null,
               AnalyzerConfigOptionsProvider optionsProvider = null)
        {
            var driver = CSharpGeneratorDriver.Create(generators, additionalTexts, parseOptions, optionsProvider);
            driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

            return new GeneratorResult((CSharpCompilation)outputCompilation, diagnostics,
                outputCompilation.SyntaxTrees.Except(compilation.SyntaxTrees, new SyntaxComparer())
                .Cast<CSharpSyntaxTree>().ToImmutableArray());
        }

        private class SyntaxComparer : IEqualityComparer<SyntaxTree>
        {
            public bool Equals(SyntaxTree x, SyntaxTree y) => x.IsEquivalentTo(y);
            public int GetHashCode(SyntaxTree obj) => obj.FilePath?.GetHashCode() ?? 0;
        }
        protected class GeneratorResult
        {
            public GeneratorResult(
             CSharpCompilation outputCompilation,
             ImmutableArray<Diagnostic> diagnostics,
             ImmutableArray<CSharpSyntaxTree> addedSyntaxTrees)
            {
                OutputCompilation = outputCompilation;
                Diagnostics = diagnostics;
                AddedSyntaxTrees = addedSyntaxTrees;
            }
            public CSharpCompilation OutputCompilation { get; }
            public ImmutableArray<Diagnostic> Diagnostics { get; }
            public ImmutableArray<CSharpSyntaxTree> AddedSyntaxTrees { get; }
        }
    }
}
