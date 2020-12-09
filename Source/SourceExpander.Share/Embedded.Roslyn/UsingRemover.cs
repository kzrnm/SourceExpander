using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SourceExpander.Roslyn
{
    internal class UsingRemover : CSharpSyntaxRewriter
    {
        public override SyntaxNode? Visit(SyntaxNode? node)
        {
            if (node.IsKind(SyntaxKind.CompilationUnit) || node.IsKind(SyntaxKind.UsingDirective))
                return base.Visit(node);
            return node;
        }
        public override SyntaxNode? VisitUsingDirective(UsingDirectiveSyntax node)
            => node.Parent.IsKind(SyntaxKind.CompilationUnit) ? null : base.VisitUsingDirective(node);
    }
}
