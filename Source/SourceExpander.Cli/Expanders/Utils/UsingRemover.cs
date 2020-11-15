using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SourceExpander.Expanders.Utils
{
    internal class UsingRemover : CSharpSyntaxRewriter
    {
        public override SyntaxNode? VisitUsingDirective(UsingDirectiveSyntax node)
            => node.Parent.IsKind(SyntaxKind.CompilationUnit) ? default : base.VisitUsingDirective(node);
    }
}
