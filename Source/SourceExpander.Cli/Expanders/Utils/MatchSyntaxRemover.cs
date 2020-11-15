using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace SourceExpander.Expanders.Utils
{
    internal class MatchSyntaxRemover : CSharpSyntaxRewriter
    {
        private readonly HashSet<SyntaxNode?> _syntaxes;
        public MatchSyntaxRemover(IEnumerable<SyntaxNode> syntaxes)
        {
            _syntaxes = new HashSet<SyntaxNode?>(syntaxes);
        }
        public override SyntaxNode? Visit(SyntaxNode? node)
            => _syntaxes.Contains(node) ? default : base.Visit(node);
    }
}
