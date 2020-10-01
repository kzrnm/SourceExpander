using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace SourceExpander
{
    public class EmbeddedGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            var compilation = context.Compilation;
            if (!compilation.HasType("SourceExpander.SourceFileInfo"))
            {
                throw new InvalidOperationException("need class SourceExpander.SourceFileInfo");
            }

            var infos = ResolveRaw(compilation,
                compilation.SyntaxTrees.Select(tree => ParseSource(compilation, tree)).ToArray()
                ).ToArray();
            Array.Sort(infos, (info1, info2) => StringComparer.OrdinalIgnoreCase.Compare(info1.FileName, info2.FileName));


            var sb = new StringBuilder();
            sb.AppendLine("using SourceExpander;");
            sb.AppendLine("internal static class ModuleInitializer{");
            sb.AppendLine("private static bool s_initialized;");
            sb.AppendLine("public static void Initialize(){");
            sb.AppendLine("if(s_initialized) return;");
            sb.AppendLine("s_initialized = true;");
            foreach (var info in infos)
            {
                sb.AppendLine("GlobalSourceFileContainer.Add(" + info.ToInitializeString() + ");");
            }
            sb.AppendLine("}}");

            if (!compilation.HasType("System.Runtime.CompilerServices.ModuleInitializerAttribute"))
            {
                sb.AppendLine(ModuleInitializerAttributeDefinition);
            }
            context.AddSource("SourceExpander.Embedded.Generated.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
        }
        private IEnumerable<SourceFileInfo> ResolveRaw(Compilation compilation, SourceFileInfoRaw[] infos)
        {
            static IEnumerable<string> GetDependencies(Compilation compilation, SourceFileInfoRaw[] infos, SourceFileInfoRaw raw)
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
                }

                return dependencies;
            }

            foreach (var raw in infos)
                yield return new SourceFileInfo
                {
                    FileName = raw.FileName,
                    TypeNames = raw.TypeNames,
                    Usings = raw.Usings,
                    CodeBody = raw.CodeBody,
                    Dependencies = GetDependencies(compilation, infos, raw),
                };
        }
        private SourceFileInfoRaw ParseSource(Compilation compilation, SyntaxTree tree)
        {
            var semanticModel = compilation.GetSemanticModel(tree, true);
            var root = (CompilationUnitSyntax)tree.GetRoot();
            var usings = root.Usings.Select(u => u.ToString().Trim()).ToArray();

            var remover = new UsingDirectiveRemover();
            var newRoot = (CompilationUnitSyntax)remover.Visit(root)!;

            var prefix = $"{compilation.AssemblyName}>";
            var fileName = tree.FilePath.Replace(compilation.ResolveCommomPrefix(), prefix);

            var typeNames = root.DescendantNodes()
                .Where(s => s is BaseTypeDeclarationSyntax || s is DelegateDeclarationSyntax)
                .Select(syntax => semanticModel.GetDeclaredSymbol(syntax)?.ToDisplayString())
                .OfType<string>()
                .Distinct()
                .ToArray();

            return new SourceFileInfoRaw(tree, fileName, typeNames, usings, MinifySpace(newRoot.ToString()));

            static string MinifySpace(string str) => System.Text.RegularExpressions.Regex.Replace(str, " +", " ");
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

        public const string ModuleInitializerAttributeDefinition = @"namespace System.Runtime.CompilerServices { internal class ModuleInitializerAttribute : System.Attribute { } }";
        public void Initialize(GeneratorInitializationContext context) { }
    }
}
