using Microsoft.CodeAnalysis.CSharp;

namespace SourceExpander
{
    public static class RoslynUtil
    {
        public static string ToLiteral(this string str)
            => SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(str)).ToFullString();
    }
}
