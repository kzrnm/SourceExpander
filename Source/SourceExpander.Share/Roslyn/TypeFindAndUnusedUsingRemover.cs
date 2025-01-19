using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace SourceExpander.Roslyn;

internal class TypeFindAndUnusedUsingRemover : CSharpSyntaxRewriter
{
    public record Result(
        ImmutableHashSet<string> DefinedTypeNames,
        ImmutableHashSet<INamedTypeSymbol> UsedTypes,
        ImmutableHashSet<string> RootUsings,
        SyntaxTree SyntaxTree);


    bool visiting = false;
#nullable disable
    protected SemanticModel SemanticModel;
    protected ImmutableArray<TextSpan> UnusedUsingSpan;
    protected CancellationToken CancellationToken;
    protected ImmutableHashSet<string>.Builder definedTypesBuilder;
    protected ImmutableHashSet<INamedTypeSymbol>.Builder usedTypesBuilder;
    protected ImmutableHashSet<string>.Builder rootUsingsBuilder;
#nullable enable
    public Result Visit(SyntaxTree syntaxTree, Compilation compilation, CancellationToken cancellationToken)
    {
        if (visiting)
            throw new InvalidOperationException("Multiple calling");
        CompilationUnitSyntax rewritedCompilationUnit;
        try
        {
            visiting = true;
            CancellationToken = cancellationToken;
            SemanticModel = compilation.GetSemanticModel(syntaxTree, ignoreAccessibility: true);
            UnusedUsingSpan = SemanticModel.GetDiagnostics(cancellationToken: cancellationToken)
                .Where(d => d.Id is "CS8019" or "CS0105" or "CS0246")
                .Select(d => d.Location.SourceSpan)
                .ToImmutableArray();
            definedTypesBuilder = ImmutableHashSet.CreateBuilder<string>();
            usedTypesBuilder = ImmutableHashSet.CreateBuilder<INamedTypeSymbol>(SymbolEqualityComparer.Default);
            rootUsingsBuilder = ImmutableHashSet.CreateBuilder<string>();
            rewritedCompilationUnit = VisitRoot();
        }
        finally
        {
            visiting = false;
        }
        return new(
            definedTypesBuilder.ToImmutable(),
            usedTypesBuilder.ToImmutable(),
            rootUsingsBuilder.ToImmutable(),
            rewritedCompilationUnit.SyntaxTree);
    }

    protected virtual CompilationUnitSyntax VisitRoot()
    {
        return (CompilationUnitSyntax)(Visit(SemanticModel.SyntaxTree.GetRoot(CancellationToken)) ?? throw new InvalidOperationException());
    }

    public override SyntaxNode? Visit(SyntaxNode? node)
    {
        if (node == null)
            return null;
        if (node is MemberDeclarationSyntax memberDeclarationSyntax
            && memberDeclarationSyntax is BaseTypeDeclarationSyntax or DelegateDeclarationSyntax)
        {
            if (!FindDeclaredType(memberDeclarationSyntax))
                return null;
        }
        var namedTypeSymbol = RoslynUtil.GetTypeNameFromSymbol(SemanticModel.GetSymbolInfo(node, CancellationToken).Symbol);
        if (namedTypeSymbol is not null)
        {
            usedTypesBuilder.Add(namedTypeSymbol);
        }
        return base.Visit(node);
    }

    /// <summary>
    /// Find Type or skip
    /// </summary>
    /// <returns>if <see langword="false"/>, ignore <paramref name="node"/></returns>
    protected virtual bool FindDeclaredType(MemberDeclarationSyntax node)
    {
        if (SemanticModel.GetDeclaredSymbol(node, CancellationToken) is not ITypeSymbol symbol)
            return false;

        var typeName = symbol?.ToDisplayString();
        if (typeName is not null)
        {
            definedTypesBuilder.Add(typeName);
            return true;
        }
        return false;
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
        if (UnusedUsingSpan.Any(s => s.Contains(node.Span)))
            return null;

        if (node.Parent.IsKind(SyntaxKind.CompilationUnit))
        {
            rootUsingsBuilder.Add(node.NormalizeWhitespace().ToString().Trim());
            return null;
        }

        return base.VisitUsingDirective(node);
    }
}
