using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace SourceExpander.Roslyn
{
    internal class MatchSyntaxRemover : CSharpSyntaxRewriter
    {
        private readonly ImmutableHashSet<SyntaxNode?> _syntaxes;
        public MatchSyntaxRemover(ImmutableHashSet<SyntaxNode?> syntaxes)
        {
            _syntaxes = syntaxes;
        }
        public override SyntaxNode? Visit(SyntaxNode? node)
            => _syntaxes.Contains(node) ? default : base.Visit(node);
    }
}
