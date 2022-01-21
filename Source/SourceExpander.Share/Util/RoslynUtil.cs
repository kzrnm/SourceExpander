using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace SourceExpander
{
    internal static class RoslynUtil
    {
        public static string ToLiteral(this string str)
            => SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(str)).ToFullString();
        public static IEnumerable<string> AllTypeNames(SemanticModel model, SyntaxTree tree, CancellationToken cancellationToken)
        {
            var distinctHashSet = new HashSet<string>();
            foreach (var symbol in AllTypeSymbol(model, tree, cancellationToken))
            {
                var name = symbol.ToDisplayString();
                if (distinctHashSet.Add(name))
                    yield return name;
            }
        }

        public static IEnumerable<INamedTypeSymbol> AllTypeSymbol(SemanticModel model, SyntaxTree tree, CancellationToken cancellationToken)
        {
            foreach (var node in tree.GetRoot(cancellationToken).DescendantNodes())
            {
                if (GetTypeNameFromSymbol(model.GetSymbolInfo(node, cancellationToken).Symbol) is { } symbol)
                    yield return symbol;
            }
        }

        public static INamedTypeSymbol? GetTypeNameFromSymbol(ISymbol? symbol)
        {
            if (symbol == null) return null;
            if (symbol is INamedTypeSymbol named)
                return named.ConstructedFrom;
            return symbol.ContainingType?.ConstructedFrom;
        }
    }
}
