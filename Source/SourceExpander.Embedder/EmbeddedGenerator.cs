using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace SourceExpander
{
    [Generator]
    public class EmbeddedGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            var compilation = (CSharpCompilation)context.Compilation;
            var infos = ResolveFiles(compilation);
            if (infos.Length == 0)
                return;

            var json = infos.ToJson();
            var gZipBase32768 = SourceFileInfoUtil.ToGZipBase32768(json);

            static string MakeAssemblyMetadataAttribute(string key, string value)
                => $"[assembly: AssemblyMetadataAttribute({key.ToLiteral()},{value.ToLiteral()})]";


            context.AddSource("EmbeddedSourceCode.Metadata.Generated.cs",
                SourceText.From(
                    "using System.Reflection;"
                    + MakeAssemblyMetadataAttribute("SourceExpander.EmbedderVersion", Assembly.GetExecutingAssembly().GetName().Version.ToString())
                    + MakeAssemblyMetadataAttribute("SourceExpander.EmbeddedSourceCode.GZipBase32768", gZipBase32768)
                , Encoding.UTF8));
        }
        public SourceFileInfo[] ResolveFiles(Compilation compilation)
        {
            var embedded = AssemblyMetadataUtil.GetEmbeddedSourceFiles(compilation);
            var commonPrefix = compilation.ResolveCommomPrefix();
            var infos = ResolveRaw(compilation,
                compilation.SyntaxTrees.Select(tree => ParseSource(compilation, tree, commonPrefix)).ToArray(),
                embedded)
                .Where(info => info.TypeNames.Any())
                .ToArray();
            Array.Sort(infos, (info1, info2) => StringComparer.OrdinalIgnoreCase.Compare(info1.FileName, info2.FileName));
            return infos;
        }
        private IEnumerable<SourceFileInfo> ResolveRaw(Compilation compilation, SourceFileInfoRaw[] infos, SourceFileInfo[] otherInfos)
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
        private SourceFileInfoRaw ParseSource(Compilation compilation, SyntaxTree tree, string commonPrefix)
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

        private static string? GetTypeNameFromSymbol(ISymbol? symbol)
        {
            if (symbol == null) return null;
            if (symbol is INamedTypeSymbol named)
            {
                return named.ConstructedFrom.ToDisplayString();
            }
            return symbol.ContainingType?.ConstructedFrom?.ToDisplayString() ?? symbol.ToDisplayString();
        }

        public void Initialize(GeneratorInitializationContext context) { }
    }
}
