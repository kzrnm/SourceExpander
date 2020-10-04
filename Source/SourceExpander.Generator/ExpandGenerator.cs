using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
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
            var embeddeds = context.Compilation.References
                .Select(context.Compilation.GetAssemblyOrModuleSymbol)
                .OfType<ISymbol>()
                .SelectMany(symbol => symbol.GetAttributes())
                .Select(GetAttributeSourceCode)
                .OfType<string>()
                .SelectMany(ParseEmbeddedJson)
                .ToArray();
            if (embeddeds.Length == 0)
            {
                var diagnosticDescriptor = new DiagnosticDescriptor("EXPAND0001", "not found embedded source", "not found embedded source", "ExpandGenerator", DiagnosticSeverity.Info, true);
                context.ReportDiagnostic(Diagnostic.Create(diagnosticDescriptor, Location.None));
                return;
            }

            context.AddSource("SourceExpander.Expanded.cs",
                SourceText.From(
                    MakeExpanded(context.Compilation.SyntaxTrees.OfType<CSharpSyntaxTree>(), context.Compilation, embeddeds),
                    Encoding.UTF8));
        }

        static string MakeExpanded(IEnumerable<CSharpSyntaxTree> trees, Compilation compilation, SourceFileInfo[] infos)
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
