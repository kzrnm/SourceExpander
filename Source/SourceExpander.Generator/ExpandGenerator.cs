using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using SourceExpander.Expanders;

namespace SourceExpander
{
    [Generator]
    public class ExpandGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            var (compilation, embeddeds) = Build((CSharpCompilation)context.Compilation);
            if (embeddeds.Length == 0)
            {
                var diagnosticDescriptor = new DiagnosticDescriptor("EXPAND0001", "not found embedded source", "not found embedded source", "ExpandGenerator", DiagnosticSeverity.Info, true);
                context.ReportDiagnostic(Diagnostic.Create(diagnosticDescriptor, Location.None));
            }

            context.AddSource("SourceExpander.Expanded.cs",
                SourceText.From(
                    MakeExpanded(compilation.SyntaxTrees.OfType<CSharpSyntaxTree>(), compilation, embeddeds),
                    Encoding.UTF8));
        }
        static (CSharpCompilation, SourceFileInfo[]) Build(CSharpCompilation compilation)
        {
            var trees = compilation.SyntaxTrees;
            foreach (var tree in trees)
            {
                var opts = tree.Options.WithDocumentationMode(DocumentationMode.Diagnose);
                compilation = compilation.ReplaceSyntaxTree(tree, tree.WithRootAndOptions(tree.GetRoot(), opts));
            }

            return (compilation, AssemblyMetadataUtil.GetEmbeddedSourceFiles(compilation).SelectMany(e => e.Sources).ToArray());
        }
        static string MakeExpanded(IEnumerable<CSharpSyntaxTree> trees, CSharpCompilation compilation, SourceFileInfo[] infos)
        {
            var sb = new StringBuilder();
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("namespace Expanded{");
            sb.AppendLine("public class SourceCode{ public string Path; public string Code; }");
            sb.AppendLine("public static class Expanded{");
            sb.AppendLine("public static IReadOnlyDictionary<string, SourceCode> Files { get; } = new Dictionary<string, SourceCode>{");
            foreach (var tree in trees)
            {
                var newCode = new CompilationExpander(tree, compilation, new SourceFileContainer(infos)).ExpandedString();
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
