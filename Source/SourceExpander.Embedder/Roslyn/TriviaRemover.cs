using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace SourceExpander.Roslyn
{
    internal class TriviaRemover : CSharpSyntaxRewriter
    {
        public TriviaRemover() { }
        public override SyntaxTrivia VisitTrivia(SyntaxTrivia trivia) => SyntaxFactory.ElasticMarker;
    }
}
