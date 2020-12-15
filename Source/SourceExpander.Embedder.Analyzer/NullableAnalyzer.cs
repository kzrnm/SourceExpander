using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SourceExpander.Diagnostics;

namespace SourceExpander
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class NullableAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(
                DiagnosticDescriptors.EMBEDDER0003_NullableProject,
                DiagnosticDescriptors.EMBEDDER0004_NullableDirective);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

            context.RegisterCompilationAction(AnalyzeCompilation);
            context.RegisterSyntaxNodeAction(AnalyzeNullableDirective,
                SyntaxKind.NullableDirectiveTrivia);
        }

        private void AnalyzeCompilation(CompilationAnalysisContext context)
        {
            if (context.Compilation.Options is not CSharpCompilationOptions opts)
                return;
            if (opts.NullableContextOptions.AnnotationsEnabled())
                context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.EMBEDDER0003_NullableProject, Location.None));
        }

        private void AnalyzeNullableDirective(SyntaxNodeAnalysisContext context)
        {
            if (context.Node is not NullableDirectiveTriviaSyntax node)
                return;

            var diagnostic = Diagnostic.Create(
                DiagnosticDescriptors.EMBEDDER0004_NullableDirective, node.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
    }
}
