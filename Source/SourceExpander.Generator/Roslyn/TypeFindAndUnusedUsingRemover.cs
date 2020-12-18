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
        private ImmutableHashSet<string>.Builder? typeNamesBuilder;
        public ImmutableHashSet<string>? TypeNames;
        public TypeFindAndUnusedUsingRemover(SemanticModel model, CancellationToken cancellationToken)
        {
            this.model = model;
            this.cancellationToken = cancellationToken;
            diagnostics = model.GetDiagnostics(cancellationToken: cancellationToken);
        }
        public override SyntaxNode? Visit(SyntaxNode? node)
        {
            if (node is null)
                return null;
            var namedTypeSymbol = RoslynUtil.GetTypeNameFromSymbol(model.GetSymbolInfo(node, cancellationToken).Symbol);
            if (namedTypeSymbol?.ToDisplayString() is string typeName)
            {
                typeNamesBuilder?.Add(typeName);
            }
            return base.Visit(node);
        }

        public override SyntaxNode? VisitCompilationUnit(CompilationUnitSyntax node)
        {
            typeNamesBuilder = ImmutableHashSet.CreateBuilder<string>();
            var r = base.VisitCompilationUnit(node);
            TypeNames = typeNamesBuilder.ToImmutable();
            return r;
        }
        public override SyntaxNode? VisitUsingDirective(UsingDirectiveSyntax node)
        {
            if (diagnostics
                .Where(d => d.Id == "CS8019" || d.Id == "CS0105" || d.Id == "CS0246")
                .Any(d => d.Location.SourceSpan.Contains(node.Span)))
                return null;
            return base.VisitUsingDirective(node);
        }
    }
}
