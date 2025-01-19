using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SourceExpander.Roslyn;

internal class EmbeddingTypeFindAndUnusedUsingRemover : TypeFindAndUnusedUsingRemover
{
    private const string SourceExpander_NotEmbeddingSourceAttributeName = "SourceExpander.NotEmbeddingSourceAttribute";
#nullable disable
    INamedTypeSymbol NotEmbeddingSourceAttributeSymbol;
#nullable enable
    protected override CompilationUnitSyntax VisitRoot()
    {
        NotEmbeddingSourceAttributeSymbol = SemanticModel.Compilation.GetTypeByMetadataName(SourceExpander_NotEmbeddingSourceAttributeName);
        return base.VisitRoot();
    }
    public override SyntaxNode? Visit(SyntaxNode? node)
    {
        if (node is MemberDeclarationSyntax declarationSyntax)
            return VistMemberDeclarationSyntax(declarationSyntax);
        return base.Visit(node);
    }

    private SyntaxNode? VistMemberDeclarationSyntax(MemberDeclarationSyntax node)
    {
        Debug.Assert(NotEmbeddingSourceAttributeSymbol != null);

        foreach (var attr in node.AttributeLists
            .SelectMany(a => a.Attributes)
            .Select(a => SemanticModel.GetTypeInfo(a).Type))
        {
            if (SymbolEqualityComparer.Default.Equals(NotEmbeddingSourceAttributeSymbol, attr))
            {
                return null;
            }
        }
        return base.Visit(node);
    }
}
