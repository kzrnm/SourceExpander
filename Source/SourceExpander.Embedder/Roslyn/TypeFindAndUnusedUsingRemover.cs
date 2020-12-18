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

        private readonly ImmutableHashSet<string>.Builder typeNamesBuilder = ImmutableHashSet.CreateBuilder<string>();
        public ImmutableHashSet<string> DefinedTypeNames() => typeNamesBuilder.ToImmutable();

        private readonly ImmutableHashSet<string>.Builder rootUsingsBuilder = ImmutableHashSet.CreateBuilder<string>();
        public ImmutableHashSet<string> RootUsings() => rootUsingsBuilder.ToImmutable();
        public TypeFindAndUnusedUsingRemover(SemanticModel model, CancellationToken cancellationToken)
        {
            this.model = model;
            this.cancellationToken = cancellationToken;
            diagnostics = model.GetDiagnostics(cancellationToken: cancellationToken);
        }

        private void FindDeclaredType(MemberDeclarationSyntax node)
        {
            var typeName = model.GetDeclaredSymbol(node, cancellationToken)?.ToDisplayString();
            if (typeName is not null)
                typeNamesBuilder.Add(typeName);
        }

        public override SyntaxNode? Visit(SyntaxNode? node)
        {
            if (node is BaseTypeDeclarationSyntax typeDeclaration)
                FindDeclaredType(typeDeclaration);
            else if (node is DelegateDeclarationSyntax delegateDeclaration)
                FindDeclaredType(delegateDeclaration);

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
