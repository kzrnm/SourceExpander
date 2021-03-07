using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using SourceExpander.Diagnostics;

namespace SourceExpander
{

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ExpandEmbeddedAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(
                DiagnosticDescriptors.EMBEDDER0005_ExpandEmbedded);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

            context.RegisterCompilationStartAction(OnCompilationStart);
        }

        private void OnCompilationStart(CompilationStartAnalysisContext context)
        {
            if (context.Compilation.GetTypeByMetadataName("SourceExpander.Expander") is { } expanderType)
            {
                var expandMethods = ImmutableHashSet.CreateRange(SymbolEqualityComparer.Default,
                    expanderType.GetMembers("Expand").OfType<IMethodSymbol>());
                context.RegisterSyntaxNodeAction(
                    ctx => AnalyzeInvocation(ctx, expandMethods),
                    SyntaxKind.InvocationExpression);
            }
        }

        private void AnalyzeInvocation(SyntaxNodeAnalysisContext context, ImmutableHashSet<ISymbol> expandMethods)
        {
            var symbol =
                context.SemanticModel.GetSymbolInfo(context.Node, context.CancellationToken)
                .Symbol;
            if (expandMethods.Contains(symbol))
            {
                var diagnostic = Diagnostic.Create(
                    DiagnosticDescriptors.EMBEDDER0005_ExpandEmbedded,
                    context.Node.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
