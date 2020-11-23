using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using SourceExpander.Roslyn;

namespace SourceExpander
{
    [Generator]
    public class ExpandGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            var (compilation, embeddedDatas) = Build((CSharpCompilation)context.Compilation);
            if (embeddedDatas.Length == 0)
            {
                context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.EXPAND0001, Location.None));
            }

            var sources = new List<SourceFileInfo>();
            foreach (var embedded in embeddedDatas)
            {
                if (embedded.EmbedderVersion > AssemblyUtil.AssemblyVersion)
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(DiagnosticDescriptors.EXPAND0002, Location.None,
                        AssemblyUtil.AssemblyVersion, embedded.AssemblyName, embedded.EmbedderVersion));
                }
                sources.AddRange(embedded.Sources);
            }

            context.AddSource("SourceExpander.Expanded.cs",
                SourceText.From(
                    MakeExpanded(compilation, sources),
                    Encoding.UTF8));
        }
        static (CSharpCompilation, EmbeddedData[]) Build(CSharpCompilation compilation)
        {
            var trees = compilation.SyntaxTrees;
            foreach (var tree in trees)
            {
                var opts = tree.Options.WithDocumentationMode(DocumentationMode.Diagnose);
                compilation = compilation.ReplaceSyntaxTree(tree, tree.WithRootAndOptions(tree.GetRoot(), opts));
            }

            return (compilation, AssemblyMetadataUtil.GetEmbeddedSourceFiles(compilation).ToArray());
        }
        static string MakeExpanded(CSharpCompilation compilation, IEnumerable<SourceFileInfo> infos)
        {
            var expander = new CompilationExpander(compilation, new SourceFileContainer(infos));
            var sb = new StringBuilder();
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("namespace SourceExpander.Expanded{");
            sb.AppendLine("public class SourceCode{ public string Path; public string Code; }");
            sb.AppendLine("public static class Expanded{");
            sb.AppendLine("public static IReadOnlyDictionary<string, SourceCode> Files { get; } = new Dictionary<string, SourceCode>{");
            foreach (var tree in compilation.SyntaxTrees)
            {
                var newCode = expander.ExpandCode(tree);
                var filePathLiteral = tree.FilePath.ToLiteral();
                sb.AppendLine($"{{{filePathLiteral}, new SourceCode{{ Path={filePathLiteral}, Code={newCode.ToLiteral()} }} }},");
            }
            sb.AppendLine("};");
            sb.AppendLine("}}");

            return sb.ToString();
        }

        public void Initialize(GeneratorInitializationContext context) { }
    }
}
