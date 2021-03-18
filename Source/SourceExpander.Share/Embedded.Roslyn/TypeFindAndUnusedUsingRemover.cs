using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SourceExpander.Roslyn
{
    internal class TypeFindAndUnusedUsingRemover : CSharpSyntaxRewriter
    {
        private readonly SemanticModel model;
        private readonly ImmutableArray<Diagnostic> diagnostics;
        private readonly CancellationToken cancellationToken;

        private readonly ImmutableHashSet<string>.Builder definedTypesBuilder = ImmutableHashSet.CreateBuilder<string>();
        public ImmutableHashSet<string> DefinedTypeNames() => definedTypesBuilder.ToImmutable();

        private readonly ImmutableHashSet<string>.Builder usedTypesBuilder = ImmutableHashSet.CreateBuilder<string>();
        public ImmutableHashSet<string> UsedTypeNames() => usedTypesBuilder.ToImmutable();

        private readonly ImmutableHashSet<string>.Builder rootUsingsBuilder = ImmutableHashSet.CreateBuilder<string>();
        public ImmutableHashSet<string> RootUsings() => rootUsingsBuilder.ToImmutable();

        private readonly INamedTypeSymbol? SkipAttributeSymbol;

        public TypeFindAndUnusedUsingRemover(SemanticModel model, CancellationToken cancellationToken)
            : this(model, null, cancellationToken) { }
        public TypeFindAndUnusedUsingRemover(SemanticModel model, INamedTypeSymbol? skipAttributeSymbol, CancellationToken cancellationToken)
        {
            this.model = model;
            this.SkipAttributeSymbol = skipAttributeSymbol;
            this.cancellationToken = cancellationToken;
            this.diagnostics = model.GetDiagnostics(cancellationToken: cancellationToken);
        }

        private bool HasNotEmbeddingSourceAttribute(SyntaxNode? node)
        {
            if (SkipAttributeSymbol is not null
                && node is MemberDeclarationSyntax declarationSyntax)
                foreach (var attributeListSyntax in declarationSyntax.AttributeLists)
                    foreach (var attribute in attributeListSyntax.Attributes)
                        if (model.GetTypeInfo(attribute).Type is ITypeSymbol atSymbol
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
            if (namedTypeSymbol?.ToDisplayString() is string typeName)
            {
                usedTypesBuilder.Add(typeName);
            }

            return base.Visit(node);
        }

        public override SyntaxNode? VisitUsingDirective(UsingDirectiveSyntax node)
        {
            if (diagnostics
                .Where(d => d.Id == "CS8019" || d.Id == "CS0105" || d.Id == "CS0246")
                .Any(d => d.Location.SourceSpan.Contains(node.Span)))
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
