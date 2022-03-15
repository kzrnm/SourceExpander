#nullable disable
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;

namespace SourceExpander
{
    public class CSharpSourceGeneratorTest<TSourceGenerator> : CSharpSourceGeneratorTest<TSourceGenerator, XUnitVerifier>
        where TSourceGenerator : ISourceGenerator, new()
    {
        public CSharpCompilationOptions CompilationOptions { get; set; } = new(OutputKind.DynamicallyLinkedLibrary);
        protected override CompilationOptions CreateCompilationOptions() => CompilationOptions;
        public CSharpParseOptions ParseOptions { get; set; } = new(kind: SourceCodeKind.Regular, documentationMode: DocumentationMode.Parse);
        protected override ParseOptions CreateParseOptions() => ParseOptions;
    }

    public class CSharpIncrementalGeneratorTest<TIncrementalGenerator> : SourceGeneratorTest<XUnitVerifier>
        where TIncrementalGenerator : IIncrementalGenerator, new()
    {
        public CSharpCompilationOptions CompilationOptions { get; set; } = new(OutputKind.DynamicallyLinkedLibrary);
        protected override CompilationOptions CreateCompilationOptions() => CompilationOptions;
        public CSharpParseOptions ParseOptions { get; set; } = new(languageVersion: LanguageVersion.CSharp9, kind: SourceCodeKind.Regular, documentationMode: DocumentationMode.Parse);
        protected override ParseOptions CreateParseOptions() => ParseOptions;
        public AnalyzerConfigOptionsProvider AnalyzerConfigOptionsProvider { get; set; }

        protected override string DefaultFileExt => "cs";
        public override string Language => LanguageNames.CSharp;
        protected override IEnumerable<ISourceGenerator> GetSourceGenerators() => new[] { new TIncrementalGenerator().AsSourceGenerator() };
        protected override GeneratorDriver CreateGeneratorDriver(Project project, ImmutableArray<ISourceGenerator> sourceGenerators)
            => CSharpGeneratorDriver.Create(
                sourceGenerators,
                project.AnalyzerOptions.AdditionalFiles,
                (CSharpParseOptions)project.ParseOptions!,
                AnalyzerConfigOptionsProvider ?? project.AnalyzerOptions.AnalyzerConfigOptionsProvider);
    }
}
