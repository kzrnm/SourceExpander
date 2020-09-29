﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SourceExpander.Embedder
{
    internal class UsingDirectiveRemover : CSharpSyntaxRewriter
    {
        public override SyntaxNode Visit(SyntaxNode node) => base.Visit(node);
        public override SyntaxTrivia VisitTrivia(SyntaxTrivia trivia) => SyntaxFactory.Space;
        public override SyntaxNode? VisitUsingDirective(UsingDirectiveSyntax? node)
            => node?.Parent?.IsKind(SyntaxKind.CompilationUnit) == true ? default : base.VisitUsingDirective(node);
    }
}
