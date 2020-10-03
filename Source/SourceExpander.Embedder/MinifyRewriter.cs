using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SourceExpander
{
    internal class MinifyRewriter : CSharpSyntaxRewriter
    {
        public override SyntaxTrivia VisitTrivia(SyntaxTrivia trivia) => SyntaxFactory.Space;
        public override SyntaxNode? VisitUsingDirective(UsingDirectiveSyntax node)
            => node.Parent.IsKind(SyntaxKind.CompilationUnit) == true ? default : base.VisitUsingDirective(node);
    }
}
