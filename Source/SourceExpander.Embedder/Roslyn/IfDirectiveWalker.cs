using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SourceExpander.Roslyn;

internal class IfDirectiveWalker() : CSharpSyntaxWalker(SyntaxWalkerDepth.StructuredTrivia)
{
    public bool HasIfDirective { get; private set; } = false;
    public bool VisitRoot(SyntaxNode node)
    {
        base.Visit(node);
        return HasIfDirective;
    }
    public override void DefaultVisit(SyntaxNode node)
    {
        if (!HasIfDirective) base.DefaultVisit(node);
    }
    public override void Visit(SyntaxNode? node)
    {
        if (!HasIfDirective) base.Visit(node);
    }
    public override void VisitIfDirectiveTrivia(IfDirectiveTriviaSyntax node)
    {
        HasIfDirective = true;
    }
}
