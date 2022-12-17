using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace SourceExpander.Roslyn
{
    internal class TypeFindAndUnusedUsingRemover
    {
        public TypeFindAndUnusedUsingRemover(SemanticModel model, CancellationToken cancellationToken) : this(model, null, cancellationToken) { }
        public TypeFindAndUnusedUsingRemover(SemanticModel model, INamedTypeSymbol? skipAttributeSymbol, CancellationToken cancellationToken)
        {
            var rewriter = new TypeFindAndUnusedUsingRemoverRewriter(model, skipAttributeSymbol, cancellationToken);
            this.CompilationUnit = (CompilationUnitSyntax)(rewriter.Visit(model.SyntaxTree.GetRoot(cancellationToken)) ?? throw new InvalidOperationException());
            this.DefinedTypeNames = rewriter.DefinedTypeNames.Value;
            this.UsedTypes = rewriter.UsedTypes.Value;
            this.RootUsings = rewriter.RootUsings.Value;
        }
        public ImmutableHashSet<string> DefinedTypeNames { get; }
        public ImmutableHashSet<INamedTypeSymbol> UsedTypes { get; }
        public ImmutableHashSet<string> RootUsings { get; }
        public CompilationUnitSyntax CompilationUnit { get; }

        private class TypeFindAndUnusedUsingRemoverRewriter : CSharpSyntaxRewriter
        {
            private bool visited;
            private readonly SemanticModel model;
            private readonly ImmutableArray<TextSpan> unusedUsingSpan;
            private readonly CancellationToken cancellationToken;

            private void ThrowIfNotVisited()
            {
                if (!visited)
                    throw new InvalidOperationException("Not Visited");
            }

            private readonly ImmutableHashSet<string>.Builder definedTypesBuilder = ImmutableHashSet.CreateBuilder<string>();
            private readonly ImmutableHashSet<INamedTypeSymbol>.Builder usedTypesBuilder = ImmutableHashSet.CreateBuilder<INamedTypeSymbol>(SymbolEqualityComparer.Default);
            private readonly ImmutableHashSet<string>.Builder rootUsingsBuilder = ImmutableHashSet.CreateBuilder<string>();
            public Lazy<ImmutableHashSet<string>> DefinedTypeNames { get; }
            public Lazy<ImmutableHashSet<INamedTypeSymbol>> UsedTypes { get; }
            public Lazy<ImmutableHashSet<string>> RootUsings { get; }

            private readonly INamedTypeSymbol? SkipAttributeSymbol;
            public TypeFindAndUnusedUsingRemoverRewriter(SemanticModel model, INamedTypeSymbol? skipAttributeSymbol, CancellationToken cancellationToken)
            {
                this.model = model;
                this.SkipAttributeSymbol = skipAttributeSymbol;
                this.cancellationToken = cancellationToken;
                this.unusedUsingSpan = model.GetDiagnostics(cancellationToken: cancellationToken)
                    .Where(d => d.Id == "CS8019" || d.Id == "CS0105" || d.Id == "CS0246")
                    .Select(d => d.Location.SourceSpan)
                    .ToImmutableArray();
                this.UsedTypes = new(() =>
                {
                    ThrowIfNotVisited();
                    return usedTypesBuilder.ToImmutable();
                });
                this.RootUsings = new(() =>
                {
                    ThrowIfNotVisited();
                    return rootUsingsBuilder.ToImmutable();
                });
                this.DefinedTypeNames = new(() =>
                {
                    ThrowIfNotVisited();
                    return definedTypesBuilder.ToImmutable();
                });
            }

            private bool HasNotEmbeddingSourceAttribute(SyntaxNode? node)
            {
                if (SkipAttributeSymbol is not null
                    && node is MemberDeclarationSyntax declarationSyntax)
                    foreach (var attributeListSyntax in declarationSyntax.AttributeLists)
                        foreach (var attribute in attributeListSyntax.Attributes)
                            if (model.GetTypeInfo(attribute, cancellationToken).Type is ITypeSymbol atSymbol
                                && SymbolEqualityComparer.Default.Equals(SkipAttributeSymbol, atSymbol))
                                return true;
                return false;
            }

            /// <summary>
            /// Find Type or skip
            /// </summary>
            /// <returns>if <see langword="false"/>, ignore <paramref name="node"/></returns>
            private bool FindDeclaredType(MemberDeclarationSyntax node)
            {
                if (model.GetDeclaredSymbol(node, cancellationToken) is not ITypeSymbol symbol)
                    return false;

                var typeName = symbol?.ToDisplayString();
                if (typeName is not null)
                {
                    definedTypesBuilder.Add(typeName);
                    return true;
                }
                return false;
            }

            public override SyntaxNode? Visit(SyntaxNode? node)
            {
                if (node == null)
                    return null;
                if (HasNotEmbeddingSourceAttribute(node))
                    return null;
                if (node is BaseTypeDeclarationSyntax typeDeclaration)
                {
                    if (!FindDeclaredType(typeDeclaration))
                        return null;
                }
                else if (node is DelegateDeclarationSyntax delegateDeclaration)
                {
                    if (!FindDeclaredType(delegateDeclaration))
                        return null;
                }
                var namedTypeSymbol = RoslynUtil.GetTypeNameFromSymbol(model.GetSymbolInfo(node, cancellationToken).Symbol);
                if (namedTypeSymbol is not null)
                {
                    usedTypesBuilder.Add(namedTypeSymbol);
                }
                try
                {
                    return base.Visit(node);
                }
                finally
                {
                    if (node is CompilationUnitSyntax)
                        visited = true;
                }
            }

#if USE_ROSLYN4
            public override SyntaxNode? VisitFileScopedNamespaceDeclaration(FileScopedNamespaceDeclarationSyntax node)
            {
                node = (FileScopedNamespaceDeclarationSyntax)base.VisitFileScopedNamespaceDeclaration(node)!;
                return SyntaxFactory.NamespaceDeclaration(node.AttributeLists, node.Modifiers, node.Name, node.Externs, node.Usings, node.Members);
            }
#endif

            public override SyntaxNode? VisitUsingDirective(UsingDirectiveSyntax node)
            {
                if (unusedUsingSpan.Any(s => s.Contains(node.Span)))
                    return null;

                if (node.Parent.IsKind(SyntaxKind.CompilationUnit))
                {
                    rootUsingsBuilder.Add(node.NormalizeWhitespace().ToString().Trim());
                    return null;
                }

                return base.VisitUsingDirective(node);
            }
        }
    }
}
