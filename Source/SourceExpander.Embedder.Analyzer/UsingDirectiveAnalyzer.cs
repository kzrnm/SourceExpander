using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SourceExpander.Diagnostics;

namespace SourceExpander
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class UsingDirectiveAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(
                DiagnosticDescriptors.EMBEDDER0001_UsingStatic,
                DiagnosticDescriptors.EMBEDDER0002_UsingAlias);
        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

            context.RegisterSyntaxNodeAction(AnalyzeUsingDirective, SyntaxKind.UsingDirective);
        }

        private void AnalyzeUsingDirective(SyntaxNodeAnalysisContext context)
        {
            if (context.Node is not UsingDirectiveSyntax node)
                return;

            // If node is in namespace, Using is safe.
            if (!node.Parent.IsKind(SyntaxKind.CompilationUnit))
                return;

            DiagnosticDescriptor diagnosticDescriptor;
            if (node.StaticKeyword.IsKind(SyntaxKind.StaticKeyword))
                diagnosticDescriptor = DiagnosticDescriptors.EMBEDDER0001_UsingStatic;
            else if (node.Alias != null)
                diagnosticDescriptor = DiagnosticDescriptors.EMBEDDER0002_UsingAlias;
            else
                return;

            var diagnostic = Diagnostic.Create(diagnosticDescriptor, node.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
    }
}
