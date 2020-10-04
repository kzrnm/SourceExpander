using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
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
            var result = new List<SourceFileInfo>();
            foreach (var reference in compilation.References)
            {
                var symbol = compilation.GetAssemblyOrModuleSymbol(reference);
                if (symbol is null) continue;
                foreach (var info in symbol.GetAttributes().Select(GetAttributeSourceCode).OfType<string>().SelectMany(ParseEmbeddedJson))
                {
                    result.Add(info);
                }
            }

            var trees = compilation.SyntaxTrees;
            foreach (var tree in trees)
            {
                var opts = tree.Options.WithDocumentationMode(DocumentationMode.Diagnose);
                compilation = compilation.ReplaceSyntaxTree(tree, tree.WithRootAndOptions(tree.GetRoot(), opts));
            }

            return (compilation, result.ToArray());
        }
        static string MakeExpanded(IEnumerable<CSharpSyntaxTree> trees, CSharpCompilation compilation, SourceFileInfo[] infos)
        {
            var sb = new StringBuilder();
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("namespace Expanded{");
            sb.AppendLine("public static class Expanded{");
            sb.AppendLine("public static IReadOnlyDictionary<string, string> Files { get; } = new Dictionary<string, string>{");
            foreach (var tree in trees)
            {
                var newCode = new CompilationExpander(tree, compilation, new SourceFileContainer(infos)).ExpandedString();
                sb.AppendLine($"{{{Quote(tree.FilePath)}, {Quote(newCode)}}},");
            }
            sb.AppendLine("};");
            sb.AppendLine("}}");

            return sb.ToString();
        }
        static string Quote(string str) => $"@\"{str.Replace("\"", "\"\"")}\"";
        static List<SourceFileInfo> ParseEmbeddedJson(string json)
        {
            using var ms = new MemoryStream(new UTF8Encoding(false).GetBytes(json));
            var serializer = new DataContractJsonSerializer(typeof(List<SourceFileInfo>));
            return (List<SourceFileInfo>)serializer.ReadObject(ms);

        }

        static string? GetAttributeSourceCode(AttributeData attr)
        {
            if (attr.AttributeClass?.ToString() != "System.Reflection.AssemblyMetadataAttribute")
                return null;
            var args = attr.ConstructorArguments;
            if (!(args.Length == 2 && args[0].Value is string key && args[1].Value is string val)) return null;
            if (key != "SourceExpander.EmbeddedSourceCode") return null;
            return val;
        }

        public void Initialize(GeneratorInitializationContext context) { }
    }
}
