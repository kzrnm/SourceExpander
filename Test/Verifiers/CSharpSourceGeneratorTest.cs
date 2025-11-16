#nullable disable
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

namespace SourceExpander
{
    public class CSharpSourceGeneratorTest<TSourceGenerator> : CSharpSourceGeneratorTest<TSourceGenerator, DefaultVerifier>
        where TSourceGenerator : new()
    {
        protected override async Task<(Compilation compilation, ImmutableArray<Diagnostic> generatorDiagnostics)> GetProjectCompilationAsync(Project project, IVerifier verifier, CancellationToken cancellationToken)
        {
            if (project.Name == DefaultTestProjectName)
                return await base.GetProjectCompilationAsync(project, verifier, cancellationToken);
            return (await project.GetCompilationAsync(cancellationToken).ConfigureAwait(false), []);
        }

        public CSharpCompilationOptions CompilationOptions { get; set; } = new(OutputKind.DynamicallyLinkedLibrary);
        protected override CompilationOptions CreateCompilationOptions() => CompilationOptions;
        public CSharpParseOptions ParseOptions { get; set; } = new(languageVersion: LanguageVersion.CSharp9, kind: SourceCodeKind.Regular, documentationMode: DocumentationMode.Parse);
        protected override ParseOptions CreateParseOptions() => ParseOptions;
        public AnalyzerConfigOptionsProvider AnalyzerConfigOptionsProvider { get; set; }
        protected override AnalyzerOptions GetAnalyzerOptions(Project project) => AnalyzerConfigOptionsProvider switch
        {
            { } optionsProvider => new AnalyzerOptions(project.AnalyzerOptions.AdditionalFiles, optionsProvider),
            _ => base.GetAnalyzerOptions(project),
        };
    }
}
