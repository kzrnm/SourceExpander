using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SourceExpander
{
    internal class DummyRewriter : CSharpSyntaxRewriter
    {
        public DummyRewriter()
        {
            var expArg = SyntaxFactory.AttributeArgument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal("EXP")));
            var conditionalArgs = SyntaxFactory.AttributeArgumentList(SyntaxFactory.SeparatedList(new[] { expArg }));
            var conditionalAttr = SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("Conditional"), conditionalArgs);
            conditionalAttributeSyntax = SyntaxFactory.AttributeList(SyntaxFactory.SeparatedList(new[] { conditionalAttr }));
        }
        private readonly AttributeListSyntax conditionalAttributeSyntax;

        private bool hasExtensionMethod;
        private bool hasVoidMethod;
        public override SyntaxNode? VisitCompilationUnit(CompilationUnitSyntax node)
        {
            hasVoidMethod = false;
            if (base.VisitCompilationUnit(node) is not CompilationUnitSyntax dec)
                return null;

            if (hasVoidMethod)
                dec = dec.AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Diagnostics")));
            return dec;
        }

        public override SyntaxNode? VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            if (node.Parent.IsKind(SyntaxKind.ClassDeclaration) && node.Modifiers.Any(SyntaxKind.PrivateKeyword))
                return null;

            hasExtensionMethod = false;
            if (base.VisitClassDeclaration(node) is not ClassDeclarationSyntax dec)
                return null;

            if (!hasExtensionMethod)
                dec = dec.WithModifiers(
                    new SyntaxTokenList(dec.Modifiers.Where(t => !t.IsKind(SyntaxKind.StaticKeyword))));
            return dec;
        }
        public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            if (!node.Modifiers.Any(SyntaxKind.PublicKeyword))
                return null;

            var attributeLists = node.AttributeLists;
            if (node.ReturnType is PredefinedTypeSyntax returnType && returnType.Keyword.IsKind(SyntaxKind.VoidKeyword))
            {
                hasVoidMethod = true;
                attributeLists = attributeLists.Add(conditionalAttributeSyntax);
            }

            var parameters = node.ParameterList.Parameters;
            if (parameters.Count > 0 && parameters[0].Modifiers.Any(SyntaxKind.ThisKeyword))
                hasExtensionMethod = true;

            var dec = SyntaxFactory.MethodDeclaration(
                attributeLists,
                node.Modifiers,
                node.ReturnType,
                node.ExplicitInterfaceSpecifier,
                node.Identifier,
                node.TypeParameterList,
                node.ParameterList,
                node.ConstraintClauses,
                CreateMinifiedBlock(node.ReturnType),
                null);
            return dec;
        }

        private BlockSyntax CreateMinifiedBlock(TypeSyntax returnType)
        {
            if (returnType is PredefinedTypeSyntax predefinedType)
            {
                switch (predefinedType.Keyword.Kind())
                {
                    case SyntaxKind.VoidKeyword:
                        return SyntaxFactory.Block();
                    case SyntaxKind.ByteKeyword:
                    case SyntaxKind.SByteKeyword:
                    case SyntaxKind.ShortKeyword:
                    case SyntaxKind.UShortKeyword:
                    case SyntaxKind.IntKeyword:
                    case SyntaxKind.UIntKeyword:
                    case SyntaxKind.LongKeyword:
                    case SyntaxKind.ULongKeyword:
                        return SyntaxFactory.Block(SyntaxFactory.ReturnStatement(
                            SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(0))
                            ));
                    case SyntaxKind.StringKeyword:
                        return SyntaxFactory.Block(SyntaxFactory.ReturnStatement(
                            SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(""))
                            ));
                }
            }
            return SyntaxFactory.Block(SyntaxFactory.ReturnStatement(
                SyntaxFactory.DefaultExpression(returnType)
                ));
        }
    }
}
