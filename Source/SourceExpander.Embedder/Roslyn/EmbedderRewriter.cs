using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;


namespace SourceExpander.Roslyn
{
    /// <summary>
    /// Rewrite by <see cref="EmbedderConfig"/>
    /// </summary>
    class EmbedderRewriter : CSharpSyntaxRewriter
    {
        private readonly SemanticModel model;
        private readonly EmbedderConfig config;
        private readonly IDiagnosticReporter reporter;
        private readonly CancellationToken cancellationToken;
        public EmbedderRewriter(SemanticModel model, EmbedderConfig config, IDiagnosticReporter reporter, CancellationToken cancellationToken)
        {
            this.model = model;
            this.config = config;
            this.reporter = reporter;
            this.cancellationToken = cancellationToken;
        }
        public override SyntaxNode? VisitUsingDirective(UsingDirectiveSyntax node)
        {
            if (node.Parent.IsKind(SyntaxKind.CompilationUnit))
            {
                DiagnosticDescriptor diagnosticDescriptor;
                if (node.StaticKeyword.IsKind(SyntaxKind.StaticKeyword))
                    diagnosticDescriptor = DiagnosticDescriptors.EMBED0009_UsingStaticDirective;
                else if (node.Alias != null)
                    diagnosticDescriptor = DiagnosticDescriptors.EMBED0010_UsingAliasDirective;
                else
                    goto Fin;

                reporter.ReportDiagnostic(Diagnostic.Create(diagnosticDescriptor, node.GetLocation()));
            }
        Fin: return base.VisitUsingDirective(node);
        }
        public override SyntaxTrivia VisitTrivia(SyntaxTrivia trivia)
        {
            if (trivia.IsKind(SyntaxKind.NullableDirectiveTrivia))
            {
                reporter.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.EMBED0008_NullableDirective, trivia.GetLocation()));
            }
            return SyntaxFactory.ElasticMarker;
        }
        public override SyntaxNode? VisitAttribute(AttributeSyntax node)
        {
            if (model.GetTypeInfo(node, cancellationToken).Type is { } typeSymbol
                && config.ExcludeAttributes.Contains(typeSymbol.ToString()))
                return null;
            return base.VisitAttribute(node);
        }

        public override SyntaxNode? VisitAttributeList(AttributeListSyntax node)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (base.VisitAttributeList(node) is AttributeListSyntax attrs && attrs.Attributes.Any())
                return attrs;
            return null;
        }
        public override SyntaxNode? VisitExpressionStatement(ExpressionStatementSyntax node)
        {
            bool IsRemovableInvocation(InvocationExpressionSyntax node)
            {
                const string System_Diagnostics_ConditionalAttribute = "System.Diagnostics.ConditionalAttribute";

                if (model.GetSymbolInfo(node, cancellationToken).Symbol is not IMethodSymbol symbol)
                    return false;
                var conditions = symbol.GetAttributes()
                    .Where(at => at.AttributeClass?.ToString() == System_Diagnostics_ConditionalAttribute)
                    .Select(at => at.ConstructorArguments[0].Value)
                    .OfType<string>();

                return config.RemoveConditional.Overlaps(conditions);
            }
            cancellationToken.ThrowIfCancellationRequested();
            var res = base.VisitExpressionStatement(node);
            if (node.Parent.IsKind(SyntaxKind.Block)
                && node.Expression is InvocationExpressionSyntax invocation
                && IsRemovableInvocation(invocation))
            {
                return null;
            }
            return res;
        }
    }
}
