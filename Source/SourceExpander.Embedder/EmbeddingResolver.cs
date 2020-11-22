using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SourceExpander.Roslyn;

namespace SourceExpander
{
    public class EmbeddingResolver
    {
        private readonly CSharpCompilation compilation;
        private readonly IDiagnosticReporter reporter;

        public EmbeddingResolver(CSharpCompilation compilation, IDiagnosticReporter reporter)
        {
            this.compilation = compilation;
            this.reporter = reporter;
        }

        public SourceFileInfo[] ResolveFiles()
        {
            var embedded = AssemblyMetadataUtil.GetEmbeddedSourceFiles(compilation);
            var commonPrefix = ResolveCommomFileNamePrefix(compilation);
            var infos = ResolveRaw(
                compilation.SyntaxTrees.Select(tree => ParseSource(tree, commonPrefix)).ToArray(),
                embedded.SelectMany(e => e.Sources).ToArray())
                .Where(info => info.TypeNames.Any())
                .ToArray();
            Array.Sort(infos, (info1, info2) => StringComparer.OrdinalIgnoreCase.Compare(info1.FileName, info2.FileName));
            return infos;
        }
        private IEnumerable<SourceFileInfo> ResolveRaw(SourceFileInfoRaw[] infos, SourceFileInfo[] otherInfos)
        {
            IEnumerable<string> GetDependencies(SourceFileInfoRaw raw)
            {
                var tree = raw.SyntaxTree;
                var root = (CompilationUnitSyntax)tree.GetRoot();

                var semanticModel = compilation.GetSemanticModel(tree, true);
                var typeQueue = new Queue<string>(root.DescendantNodes()
                    .Select(s => GetTypeNameFromSymbol(semanticModel.GetSymbolInfo(s).Symbol?.OriginalDefinition))
                    .OfType<string>()
                    .Distinct());

                var added = new HashSet<string>(raw.TypeNames);
                var dependencies = new HashSet<string>();
                while (typeQueue.Count > 0)
                {
                    var typeName = typeQueue.Dequeue();
                    if (!added.Add(typeName)) continue;

                    dependencies.UnionWith(infos.Where(s => s.TypeNames != null && s.TypeNames.Contains(typeName)).Select(s => s.FileName));
                    dependencies.UnionWith(otherInfos.Where(s => s.TypeNames?.Contains(typeName) == true).Select(s => s.FileName).OfType<string>());
                }

                return dependencies;
            }

            foreach (var raw in infos)
                yield return new SourceFileInfo
                (
                    raw.FileName,
                    raw.TypeNames,
                    raw.Usings,
                    GetDependencies(raw),
                    raw.CodeBody
                );
        }
        private SourceFileInfoRaw ParseSource(SyntaxTree tree, string commonPrefix)
        {
            var semanticModel = compilation.GetSemanticModel(tree, true);
            var root = (CompilationUnitSyntax)tree.GetRoot();
            var usings = root.Usings.Select(u => u.ToString().Trim()).ToArray();

            var remover = new MinifyRewriter();
            var newRoot = (CompilationUnitSyntax)remover.Visit(root)!;

            var prefix = $"{compilation.AssemblyName}>";
            var fileName = string.IsNullOrEmpty(commonPrefix) ?
                prefix + tree.FilePath :
                tree.FilePath.Replace(commonPrefix, prefix);

            var typeNames = root.DescendantNodes()
                .Where(s => s is BaseTypeDeclarationSyntax || s is DelegateDeclarationSyntax)
                .Select(syntax => semanticModel.GetDeclaredSymbol(syntax)?.ToDisplayString())
                .OfType<string>()
                .Distinct()
                .ToArray();

            return new SourceFileInfoRaw(tree, fileName, typeNames, usings,
                remover.Visit(CSharpSyntaxTree.ParseText(newRoot.ToString()).GetRoot())!.ToString());
        }

        public bool HasType(string typeFullName)
            => compilation.GetTypeByMetadataName(typeFullName) != null;

        private static string? GetTypeNameFromSymbol(ISymbol? symbol)
        {
            if (symbol == null) return null;
            if (symbol is INamedTypeSymbol named)
            {
                return named.ConstructedFrom.ToDisplayString();
            }
            return symbol.ContainingType?.ConstructedFrom?.ToDisplayString() ?? symbol.ToDisplayString();
        }

        public static string ResolveCommomFileNamePrefix(Compilation compilation)
            => ResolveCommomPrefix(compilation.SyntaxTrees.Select(tree => tree.FilePath));
        public static string ResolveCommomPrefix(IEnumerable<string> strs)
        {
            var sorted = new SortedSet<string>(strs, StringComparer.Ordinal);
            if (sorted.Count < 1) return "";
            if (sorted.Count < 2)
            {
                var p = sorted.Min;
                var name = Path.GetFileName(p);
                if (!p.EndsWith(name))
                    return "";

                return p.Substring(0, p.Length - name.Length);
            }

            var min = sorted.Min;
            var max = sorted.Max;

            for (int i = 0; i < min.Length && i < max.Length; i++)
            {
                if (min[i] != max[i])
                    return min.Substring(0, i);
            }
            return min;
        }
    }
}
