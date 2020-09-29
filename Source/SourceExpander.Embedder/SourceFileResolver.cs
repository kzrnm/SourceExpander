using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SourceExpander.Embedder
{
    public class SourceFileResolver
    {
        private string ModuleName { get; }
        private Compilation Compilation { get; }
        private SourceFileInfoRaw[] Infos { get; }

        public SourceFileResolver(string moduleName, IEnumerable<string> sourceFilePath)
        : this(moduleName, sourceFilePath.Select(SourceFileInfoRaw.ParseFile)) { }

        public SourceFileResolver(string moduleName, IEnumerable<SourceFileInfoRaw> infos)
        {
            ModuleName = moduleName;
            Infos = infos.ToArray();


            Array.Sort(Infos, (info1, info2) => StringComparer.OrdinalIgnoreCase.Compare(info1.FilePath, info2.FilePath));

            Compilation = CSharpCompilation.Create("Compilation", Infos.Select(info => info.SyntaxTree), GetMetadataReferences());

            var commonPrefixLength = ResolveCommomPrefix(Infos.Select(info => info.FilePath));
            foreach (var info in Infos)
            {
                info.FilePath = $"{ModuleName}>{info.FilePath.Substring(commonPrefixLength)}";
                info.ResolveType(Compilation);
            }
        }

        public SourceFileInfo[] Resolve()
        {
            var result = new SourceFileInfo[Infos.Length];

            for (int i = 0; i < result.Length; i++)
            {
                var info = Infos[i];
                result[i] = new SourceFileInfo
                {
                    FileName = info.FilePath,
                    TypeNames = info.TypeNames,
                    CodeBody = info.CodeBody,
                    Usings = info.Usings,
                    Dependencies = GetDependencies(info)
                };
            }
            return result;
        }


        private static int ResolveCommomPrefix(IEnumerable<string> strs)
        {
            var arr = strs.ToArray();
            if (arr.Length < 2) return 0;
            Array.Sort(arr, StringComparer.Ordinal);
            var min = arr[0];
            var max = arr[arr.Length - 1];

            for (int i = 0; i < min.Length && i < max.Length; i++)
            {
                if (min[i] != max[i])
                    return i;
            }
            return Math.Min(min.Length, max.Length);
        }

        private static IEnumerable<MetadataReference> GetMetadataReferences()
            => Directory.EnumerateFiles(Path.GetDirectoryName(typeof(object).Assembly.Location), "*.dll")
            .Select(l => MetadataReference.CreateFromFile(l));

        private IEnumerable<string> GetDependencies(SourceFileInfoRaw raw)
        {
            var tree = raw.SyntaxTree;
            var root = (CompilationUnitSyntax)tree.GetRoot();

            var semanticModel = Compilation.GetSemanticModel(tree, true);
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

                dependencies.UnionWith(Infos.Where(s => s.TypeNames != null && s.TypeNames.Contains(typeName)).Select(s => s.FilePath));
            }

            return dependencies;
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
    }
}
