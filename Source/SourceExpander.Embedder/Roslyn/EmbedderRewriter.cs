using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;


namespace SourceExpander.Roslyn;

using static SyntaxFactory;

/// <summary>
/// Rewrite by <see cref="EmbedderConfig"/>
/// </summary>
class EmbedderRewriter(SemanticModel model, EmbedderConfig config, IDiagnosticReporter reporter, CancellationToken cancellationToken) : CSharpSyntaxRewriter
{
    private readonly SemanticModel semanticModel = model;
    private readonly EmbedderConfig config = config;
    private readonly IDiagnosticReporter reporter = reporter;
    private readonly CancellationToken cancellationToken = cancellationToken;


    public override SyntaxNode? Visit(SyntaxNode? node)
    {
        if (node is MemberDeclarationSyntax memberDeclaration)
            return VisitMemberDeclaration(memberDeclaration);
        return base.Visit(node);
    }

    protected virtual SyntaxNode? VisitMemberDeclaration(MemberDeclarationSyntax node)
        => HasNotEmbeddingSourceAttribute(node.AttributeLists) ? null : base.Visit(node);
    public override SyntaxNode? VisitLocalFunctionStatement(LocalFunctionStatementSyntax node)
        => HasNotEmbeddingSourceAttribute(node.AttributeLists) ? null : base.VisitLocalFunctionStatement(node);
    protected bool HasNotEmbeddingSourceAttribute(SyntaxList<AttributeListSyntax> attributeLists)
    {
        const string SourceExpander_NotEmbeddingSourceAttributeName = "SourceExpander.NotEmbeddingSourceAttribute";
        foreach (var attrSyntax in attributeLists.SelectMany(a => a.Attributes))
            if (semanticModel.GetTypeInfo(attrSyntax).Type?.ToString() is SourceExpander_NotEmbeddingSourceAttributeName)
                return true;
        return false;
    }

    public override SyntaxTrivia VisitTrivia(SyntaxTrivia trivia) => ElasticMarker;

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
        var typeName = semanticModel.GetTypeInfo(node, cancellationToken).Type?.ToString();
        if (typeName is not null && config.ExcludeAttributes.Contains(typeName))
            return null;
        return base.VisitAttribute(node);
    }

    public override SyntaxNode? VisitAttributeList(AttributeListSyntax node)
    {
        if (node.Target?.Identifier is { } target && (target.IsKind(SyntaxKind.AssemblyKeyword) || target.IsKind(SyntaxKind.ModuleKeyword)))
            return null;
        if (base.VisitAttributeList(node) is AttributeListSyntax attrs && attrs.Attributes.Count > 0)
        {
            if (node.Attributes.Count == attrs.Attributes.Count)
                return attrs;
            return AttributeList(SeparatedList(attrs.Attributes));
        }
        return null;
    }

    public override SyntaxNode? VisitExpressionStatement(ExpressionStatementSyntax node)
    {
        bool IsRemovable(ExpressionStatementSyntax node)
        {
            const string System_Diagnostics_ConditionalAttribute = "System.Diagnostics.ConditionalAttribute";

            cancellationToken.ThrowIfCancellationRequested();

            if (!node.Parent.IsKind(SyntaxKind.Block)
                || node.Expression is not InvocationExpressionSyntax invocation
                || semanticModel.GetSymbolInfo(invocation, cancellationToken).Symbol is not IMethodSymbol symbol)
                return false;

            cancellationToken.ThrowIfCancellationRequested();

            var conditions = symbol.GetAttributes()
                .Where(at => at.AttributeClass?.ToString() == System_Diagnostics_ConditionalAttribute)
                .Select(at => at.ConstructorArguments[0].Value)
                .OfType<string>();

            cancellationToken.ThrowIfCancellationRequested();

            return config.RemoveConditional.Overlaps(conditions);
        }
        return IsRemovable(node) ? null : base.VisitExpressionStatement(node);
    }
}
