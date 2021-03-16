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
            : base(true)
        {
            this.model = model;
            this.config = config;
            this.reporter = reporter;
            this.cancellationToken = cancellationToken;
        }

        public override SyntaxNode? VisitNullableDirectiveTrivia(NullableDirectiveTriviaSyntax node)
        {
            reporter.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.EMBED0008_NullableDirective, node.GetLocation()));
            return base.VisitNullableDirectiveTrivia(node);
        }
        public override SyntaxNode? VisitAttribute(AttributeSyntax node)
        {
            if (model.GetTypeInfo(node).Type is { } typeSymbol
                && config.ExcludeAttributes.Contains(typeSymbol.ToString()))
                return null;
            return base.VisitAttribute(node);
        }

        public override SyntaxNode? VisitAttributeList(AttributeListSyntax node)
        {
            if (base.VisitAttributeList(node) is AttributeListSyntax attrs && attrs.Attributes.Count > 0)
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
