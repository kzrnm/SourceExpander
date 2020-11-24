using System.Collections.Generic;
using System;
using System.Linq;
using System.Reflection;
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


            if (!HasCoreReference(context.Compilation.ReferencedAssemblyNames))
            {
                var assembly = Assembly.GetExecutingAssembly();
                using var stream = assembly.GetManifestResourceStream("SourceCode.cs");
                context.AddSource("SourceExpander.SourceCode.cs",
                   SourceText.From(stream, Encoding.UTF8));
            }

            context.AddSource("SourceExpander.Expanded.cs",
                SourceText.From(
                    MakeExpanded(loader),
                    Encoding.UTF8));
        }
        static bool HasCoreReference(IEnumerable<AssemblyIdentity> referencedAssemblyNames)
            => referencedAssemblyNames.Any(a => a.Name.Equals("SourceExpander.Core", StringComparison.OrdinalIgnoreCase));

        static string MakeSourceCodeLiteral(string pathLiteral, string codeLiteral)
            => $"new SourceCode(path: {pathLiteral}, code: {codeLiteral})";

        static string MakeExpanded(EmbeddedLoader loader)//, bool hasCoreReference)
        {
            var sb = new StringBuilder();
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("namespace SourceExpander.Expanded{");
            sb.AppendLine("public static class ExpandedContainer{");
            sb.AppendLine("public static IReadOnlyDictionary<string, SourceCode> Files { get; } = new Dictionary<string, SourceCode>{");
            foreach (var (path, code) in loader.EnumerateExpandedCodes())
            {
                var filePathLiteral = path.ToLiteral();
                sb.AppendLine("{"
                    + filePathLiteral + ", "
                    + MakeSourceCodeLiteral(filePathLiteral, code.ToLiteral())
                    + " },");
            }
            sb.AppendLine("};");
            sb.AppendLine("}}");

            return sb.ToString();
        }

        public void Initialize(GeneratorInitializationContext context) { }
    }
}
