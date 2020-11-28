using System;
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


            if (!HasCoreReference(context.Compilation.ReferencedAssemblyNames))
            {
                context.AddSource("SourceExpander.SourceCode.cs",
                   SourceText.From(EmbeddingCore.SourceCodeClassCode, Encoding.UTF8));
            }

            context.AddSource("SourceExpander.Expanded.cs",
                SourceText.From(
                    CreateExpanded(loader),
                    Encoding.UTF8));
        }
        static bool HasCoreReference(IEnumerable<AssemblyIdentity> referencedAssemblyNames)
            => referencedAssemblyNames.Any(a => a.Name.Equals("SourceExpander.Core", StringComparison.OrdinalIgnoreCase));


        static string CreateExpanded(EmbeddedLoader loader)
        {
            static void CreateSourceCodeLiteral(StringBuilder sb, string pathLiteral, string codeLiteral)
                => sb.Append("SourceCode.FromDictionary(new Dictionary<string,object>{")
                  .AppendDicElement("\"path\"", pathLiteral)
                  .AppendDicElement("\"code\"", codeLiteral)
                  .Append("})");

            var sb = new StringBuilder();
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("namespace SourceExpander.Expanded{");
            sb.AppendLine("public static class ExpandedContainer{");
            sb.AppendLine("public static IReadOnlyDictionary<string, SourceCode> Files { get; } = new Dictionary<string, SourceCode>{");
            foreach (var (path, code) in loader.EnumerateExpandedCodes())
            {
                var filePathLiteral = path.ToLiteral();
                sb.AppendDicElement(filePathLiteral, sb => CreateSourceCodeLiteral(sb, filePathLiteral, code.ToLiteral()));
                sb.AppendLine();
            }
            sb.AppendLine("};");
            sb.AppendLine("}}");

            return sb.ToString();
        }

        public void Initialize(GeneratorInitializationContext context) { }
    }
}
