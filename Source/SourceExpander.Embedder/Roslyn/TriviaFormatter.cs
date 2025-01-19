using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SourceExpander.Roslyn
{
    internal class TriviaFormatter(MinifyLevel minifyLevel) : CSharpSyntaxRewriter(false)
    {
        public MinifyLevel MinifyLevel { get; } = minifyLevel;

        static TriviaFormatter()
        {
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

            var preEqualsBuilder = ImmutableHashSet.CreateBuilder<SyntaxKind>();
            preEqualsBuilder.Add(SyntaxKind.ExclamationToken);
            preEqualsBuilder.Add(SyntaxKind.EqualsToken);
            preEqualsBuilder.Add(SyntaxKind.LessThanToken);
            preEqualsBuilder.Add(SyntaxKind.LessThanLessThanToken);
            preEqualsBuilder.Add(SyntaxKind.GreaterThanToken);
            preEqualsBuilder.Add(SyntaxKind.GreaterThanGreaterThanToken);
            preEqualsBuilder.Add(SyntaxKind.SlashToken);
            preEqualsBuilder.Add(SyntaxKind.AsteriskToken);
            preEqualsBuilder.Add(SyntaxKind.BarToken);
            preEqualsBuilder.Add(SyntaxKind.AmpersandToken);
            preEqualsBuilder.Add(SyntaxKind.PlusToken);
            preEqualsBuilder.Add(SyntaxKind.MinusToken);
            preEqualsBuilder.Add(SyntaxKind.CaretToken);
            preEqualsBuilder.Add(SyntaxKind.PercentToken);
            preEqualsBuilder.Add(SyntaxKind.QuestionQuestionToken);
            PreEqualsToken = preEqualsBuilder.ToImmutable();
        }
        public static ImmutableHashSet<SyntaxKind> SplitToken { get; }
        public static ImmutableHashSet<SyntaxKind> PreEqualsToken { get; }
        public override SyntaxNode? VisitCompilationUnit(CompilationUnitSyntax node)
        {
            return base.VisitCompilationUnit(MinifyLevel switch
            {
                MinifyLevel.Off => node.NormalizeWhitespace(eol: "\n"),
                MinifyLevel.Full => node.NormalizeWhitespace("", " "),
                _ => node.NormalizeWhitespace("", " "),
            });
        }

        public override SyntaxNode? VisitAttributeList(AttributeListSyntax node)
            => base.VisitAttributeList(node) switch
            {
                AttributeListSyntax { Attributes.Count: 0 } => null,
                var node2 => node2,
            };

        public override SyntaxToken VisitToken(SyntaxToken token)
        {
            var res = base.VisitToken(token);
            if (MinifyLevel != MinifyLevel.Full) return res;
            var kind = token.Kind();
            var previous = token.GetPreviousToken().Kind();
            var next = token.GetNextToken().Kind();

            // LeadingTrivia
            switch (kind)
            {
                case SyntaxKind.EqualsGreaterThanToken:
                    if (PreEqualsToken.Contains(previous))
                        return res.WithLeadingTrivia(SyntaxFactory.Space).WithTrailingTrivia(SyntaxFactory.ElasticMarker);
                    return res.WithoutTrivia();
            }

            if (SplitToken.Contains(kind))
                return res.WithoutTrivia();
            if (SplitToken.Contains(previous))
                res = res.WithLeadingTrivia(SyntaxFactory.ElasticMarker);
            if (SplitToken.Contains(next))
                res = res.WithTrailingTrivia(SyntaxFactory.ElasticMarker);

            return res;
        }

        public override SyntaxNode? VisitPostfixUnaryExpression(PostfixUnaryExpressionSyntax node)
        {
            var res = base.VisitPostfixUnaryExpression(node);
            if (MinifyLevel != MinifyLevel.Full) return res;
            var origTrivia = node.GetTrailingTrivia();
            return res?.WithTrailingTrivia(origTrivia);
        }

        public override SyntaxNode? VisitPrefixUnaryExpression(PrefixUnaryExpressionSyntax node)
        {
            var res = base.VisitPrefixUnaryExpression(node);
            if (MinifyLevel != MinifyLevel.Full) return res;
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
