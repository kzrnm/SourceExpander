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
            var loader = new EmbeddedLoader((CSharpCompilation)context.Compilation, new DiagnosticReporter(context));
            if (loader.IsEmbeddedEmpty)
                context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.EXPAND0001, Location.None));

            context.AddSource("SourceExpander.Expanded.cs",
                SourceText.From(
                    MakeExpanded(loader),
                    Encoding.UTF8));
        }

        static string MakeExpanded(EmbeddedLoader loader)//, bool hasCoreReference)
        {
            var sb = new StringBuilder();
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("namespace SourceExpander.Expanded{");
            sb.AppendLine("public class SourceCode{ public string Path; public string Code; }");
            sb.AppendLine("public static class Expanded{");
            sb.AppendLine("public static IReadOnlyDictionary<string, SourceCode> Files { get; } = new Dictionary<string, SourceCode>{");
            foreach (var (path, code) in loader.EnumerateExpandedCodes())
            {
                var filePathLiteral = path.ToLiteral();
                sb.AppendLine($"{{{filePathLiteral}, new SourceCode{{ Path={filePathLiteral}, Code={code.ToLiteral()} }} }},");
            }
            sb.AppendLine("};");
            sb.AppendLine("}}");

            return sb.ToString();
        }

        public void Initialize(GeneratorInitializationContext context) { }
    }
}
