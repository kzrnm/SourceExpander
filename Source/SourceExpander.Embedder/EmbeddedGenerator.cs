using System;
using System.Collections.Generic;
using System.Linq;
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

            var json = infos.ToJson();
            context.AddSource("EmbeddedSourceCode.Metadata.Generated.cs",
                SourceText.From($"[assembly: System.Reflection.AssemblyMetadataAttribute(\"SourceExpander.EmbeddedSourceCode\", @\"{json.Replace("\"", "\"\"")}\")]", Encoding.UTF8));

            // embedding code
            if (!compilation.HasType("SourceExpander.SourceFileInfo"))
            {
                var desc = new DiagnosticDescriptor("EMBED0001",
                    "need class SourceExpander.SourceFileInfo",
                    "need class SourceExpander.SourceFileInfo",
                    "EmbeddedGenerator",
                    DiagnosticSeverity.Info,
                    true);
                context.ReportDiagnostic(Diagnostic.Create(desc, Location.None));
            }
            else
            {
                context.AddSource("SourceExpander.Embedded.Generated.cs", CreateModuleInitializer(compilation, infos));
            }


            static SourceText CreateModuleInitializer(CSharpCompilation compilation, SourceFileInfo[] infos)
            {
                var useInternalModuleInitializer = compilation.LanguageVersion.MapSpecifiedToEffectiveVersion() >= LanguageVersion.CSharp9 && !compilation.HasType("System.Runtime.CompilerServices.ModuleInitializerAttribute");
                var sb = new StringBuilder();
                sb.AppendLine("using SourceExpander;");
                sb.AppendLine("namespace SourceExpander.EmbeddedGenerator{");
                sb.AppendLine("internal static class ModuleInitializer{");
                sb.AppendLine("public static SourceFileInfo[] sourceFileInfos = new SourceFileInfo[]{");
                foreach (var info in infos)
                    sb.AppendLine(info.ToInitializeString() + ",");
                sb.AppendLine("};");
                sb.AppendLine("private static bool s_initialized = false;");
                if (useInternalModuleInitializer)
                    sb.AppendLine("[System.Runtime.CompilerServices.ModuleInitializer]");
                sb.AppendLine("public static void Initialize(){");
                sb.AppendLine("if(s_initialized) return;");
                sb.AppendLine("s_initialized = true;");
                sb.AppendLine("GlobalSourceFileContainer.Instance.AddLazy(() => sourceFileInfos);");
                sb.AppendLine("}}}");

                if (useInternalModuleInitializer)
                {
                    const string ModuleInitializerAttributeDefinition = @"namespace System.Runtime.CompilerServices { internal class ModuleInitializerAttribute : System.Attribute { } }";
                    sb.AppendLine(ModuleInitializerAttributeDefinition);
                }
                return SourceText.From(sb.ToString(), Encoding.UTF8);
            }
        }

        public SourceFileInfo[] ResolveFiles(Compilation compilation)
        {
            var infos = ResolveRaw(compilation,
                compilation.SyntaxTrees.Select(tree => ParseSource(compilation, tree)).ToArray()
                )
                .Where(info => info.TypeNames.Any())
                .ToArray();
            Array.Sort(infos, (info1, info2) => StringComparer.OrdinalIgnoreCase.Compare(info1.FileName, info2.FileName));
            return infos;
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

            var remover = new MinifyRewriter();
            var newRoot = (CompilationUnitSyntax)remover.Visit(root)!;

            var prefix = $"{compilation.AssemblyName}>";
            var commonPrefix = compilation.ResolveCommomPrefix();
            var fileName = string.IsNullOrEmpty(commonPrefix) ? 
                prefix + tree.FilePath :
                tree.FilePath.Replace(commonPrefix, prefix);

            var typeNames = root.DescendantNodes()
                .Where(s => s is BaseTypeDeclarationSyntax || s is DelegateDeclarationSyntax)
                .Select(syntax => semanticModel.GetDeclaredSymbol(syntax)?.ToDisplayString())
                .OfType<string>()
                .Distinct()
                .ToArray();

            return new SourceFileInfoRaw(tree, fileName, typeNames, usings, MinifySpace(newRoot.ToString()));

            static string MinifySpace(string str)
            {
                bool inWhiteSpace = false;
                var sb = new StringBuilder(str.Length);
                for (int i = 0; i < str.Length; i++)
                {
                    if (str[i] == ' ')
                    {
                        if (!inWhiteSpace) sb.Append(str[i]);
                        inWhiteSpace = true;
                    }
                    else
                    {
                        sb.Append(str[i]);
                        inWhiteSpace = false;
                    }
                }
                return sb.ToString();
            }
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
