﻿using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;


namespace SourceExpander.Roslyn
{
    /// <summary>
    /// Rewrite by <see cref="EmbedderConfig"/>
    /// </summary>
    class EmbedderRewriter(SemanticModel model, EmbedderConfig config, IDiagnosticReporter reporter, CancellationToken cancellationToken) : CSharpSyntaxRewriter
    {
        private readonly SemanticModel model = model;
        private readonly EmbedderConfig config = config;
        private readonly IDiagnosticReporter reporter = reporter;
        private readonly CancellationToken cancellationToken = cancellationToken;

        public override SyntaxTrivia VisitTrivia(SyntaxTrivia trivia)
        {
            return SyntaxFactory.ElasticMarker;
        }
        public override SyntaxNode? VisitUsingDirective(UsingDirectiveSyntax node)
        {
            if (node.Parent.IsKind(SyntaxKind.CompilationUnit))
            {
                Diagnostic diagnostic;
                if (node.StaticKeyword.IsKind(SyntaxKind.StaticKeyword))
                    diagnostic = DiagnosticDescriptors.EMBED0009_UsingStaticDirective(node.GetLocation());
                else if (node.Alias != null)
                    diagnostic = DiagnosticDescriptors.EMBED0010_UsingAliasDirective(node.GetLocation());
                else
                    goto Fin;

                reporter.ReportDiagnostic(diagnostic);
            }
        Fin: return base.VisitUsingDirective(node);
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
            if (node.Target?.Identifier is { } target && (target.IsKind(SyntaxKind.AssemblyKeyword) || target.IsKind(SyntaxKind.ModuleKeyword)))
                return null;
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
