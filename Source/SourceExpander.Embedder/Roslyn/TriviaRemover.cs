using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SourceExpander.Roslyn
{
    public class TriviaRemover : CSharpSyntaxRewriter
    {
        private readonly EmbedderConfig config;
        public TriviaRemover(EmbedderConfig config)
        {
            this.config = config;

            var splitBuilder = ImmutableHashSet.CreateBuilder<SyntaxKind>();
            splitBuilder.Add(SyntaxKind.TildeToken);
            splitBuilder.Add(SyntaxKind.ExclamationToken);
            splitBuilder.Add(SyntaxKind.PercentToken);
            splitBuilder.Add(SyntaxKind.CaretToken);
            splitBuilder.Add(SyntaxKind.AmpersandToken);
            splitBuilder.Add(SyntaxKind.AsteriskToken);
            splitBuilder.Add(SyntaxKind.OpenParenToken);
            splitBuilder.Add(SyntaxKind.CloseParenToken);
            splitBuilder.Add(SyntaxKind.MinusToken);
            splitBuilder.Add(SyntaxKind.PlusToken);
            splitBuilder.Add(SyntaxKind.EqualsToken);
            splitBuilder.Add(SyntaxKind.OpenBraceToken);
            splitBuilder.Add(SyntaxKind.CloseBraceToken);
            splitBuilder.Add(SyntaxKind.OpenBracketToken);
            splitBuilder.Add(SyntaxKind.CloseBracketToken);
            splitBuilder.Add(SyntaxKind.BarToken);
            splitBuilder.Add(SyntaxKind.BackslashToken);
            splitBuilder.Add(SyntaxKind.ColonToken);
            splitBuilder.Add(SyntaxKind.SemicolonToken);
            splitBuilder.Add(SyntaxKind.DoubleQuoteToken);
            splitBuilder.Add(SyntaxKind.SingleQuoteToken);
            splitBuilder.Add(SyntaxKind.LessThanToken);
            splitBuilder.Add(SyntaxKind.CommaToken);
            splitBuilder.Add(SyntaxKind.GreaterThanToken);
            splitBuilder.Add(SyntaxKind.DotToken);
            splitBuilder.Add(SyntaxKind.QuestionToken);
            splitBuilder.Add(SyntaxKind.SlashToken);
            splitBuilder.Add(SyntaxKind.DotDotToken);
            splitBuilder.Add(SyntaxKind.BarBarToken);
            splitBuilder.Add(SyntaxKind.AmpersandAmpersandToken);
            //splitBuilder.Add(SyntaxKind.MinusMinusToken);
            //splitBuilder.Add(SyntaxKind.PlusPlusToken);
            splitBuilder.Add(SyntaxKind.ColonColonToken);
            splitBuilder.Add(SyntaxKind.QuestionQuestionToken);
            splitBuilder.Add(SyntaxKind.MinusGreaterThanToken);
            splitBuilder.Add(SyntaxKind.ExclamationEqualsToken);
            splitBuilder.Add(SyntaxKind.EqualsEqualsToken);
            splitBuilder.Add(SyntaxKind.EqualsGreaterThanToken);
            splitBuilder.Add(SyntaxKind.LessThanEqualsToken);
            splitBuilder.Add(SyntaxKind.LessThanLessThanToken);
            splitBuilder.Add(SyntaxKind.LessThanLessThanEqualsToken);
            splitBuilder.Add(SyntaxKind.GreaterThanEqualsToken);
            splitBuilder.Add(SyntaxKind.GreaterThanGreaterThanToken);
            splitBuilder.Add(SyntaxKind.GreaterThanGreaterThanEqualsToken);
            splitBuilder.Add(SyntaxKind.SlashEqualsToken);
            splitBuilder.Add(SyntaxKind.AsteriskEqualsToken);
            splitBuilder.Add(SyntaxKind.BarEqualsToken);
            splitBuilder.Add(SyntaxKind.AmpersandEqualsToken);
            splitBuilder.Add(SyntaxKind.PlusEqualsToken);
            splitBuilder.Add(SyntaxKind.MinusEqualsToken);
            splitBuilder.Add(SyntaxKind.CaretEqualsToken);
            splitBuilder.Add(SyntaxKind.PercentEqualsToken);
            splitBuilder.Add(SyntaxKind.QuestionQuestionEqualsToken);

            SplitToken = splitBuilder.ToImmutable();
        }
        public ImmutableHashSet<SyntaxKind> SplitToken { get; }
        public override SyntaxNode? Visit(SyntaxNode? node)
        {
            var res = base.Visit(node);
            if (res is null)
                return null;
            if (config.AddTriviaKinds.Contains(res.Kind()))
                return res.WithLeadingTrivia(SyntaxFactory.Space).WithTrailingTrivia(SyntaxFactory.Space);
            return res;
        }
        public override SyntaxToken VisitToken(SyntaxToken token)
        {
            var res = base.VisitToken(token);

            if (config.AddTriviaKinds.Contains(token.Kind()))
                return res
                    .WithLeadingTrivia(SyntaxFactory.Space)
                    .WithTrailingTrivia(SyntaxFactory.Space);

            if (SplitToken.Contains(token.Kind()))
                return res.WithoutTrivia();

            if (SplitToken.Contains(token.GetPreviousToken().Kind()))
                res = res.WithLeadingTrivia(SyntaxFactory.ElasticMarker);

            if (SplitToken.Contains(token.GetNextToken().Kind()))
                res = res.WithTrailingTrivia(SyntaxFactory.ElasticMarker);

            return res;
        }

        public override SyntaxNode? VisitPostfixUnaryExpression(PostfixUnaryExpressionSyntax node)
        {
            var origTrivia = node.GetTrailingTrivia();
            return base.VisitPostfixUnaryExpression(node)?.WithTrailingTrivia(origTrivia);
        }

        public override SyntaxNode? VisitPrefixUnaryExpression(PrefixUnaryExpressionSyntax node)
        {
            var res = base.VisitPrefixUnaryExpression(node);
            if (node.IsKind(SyntaxKind.PreDecrementExpression)
                || node.IsKind(SyntaxKind.PreIncrementExpression)
                || node.IsKind(SyntaxKind.PointerIndirectionExpression))
            {
                var previous = node.OperatorToken.GetPreviousToken();
                if (!previous.IsKind(SyntaxKind.SemicolonToken) && !previous.IsKind(SyntaxKind.ColonToken))
                    return res?.WithLeadingTrivia(SyntaxFactory.Space);
            }
            return res;
        }

    }
}
