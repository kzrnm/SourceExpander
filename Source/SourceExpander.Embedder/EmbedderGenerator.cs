using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using SourceExpander.Roslyn;

namespace SourceExpander
{
    [Generator]
    public class EmbedderGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context) { }
        public void Execute(GeneratorExecutionContext context)
        {
            var infos = new EmbeddingResolver((CSharpCompilation)context.Compilation, new DiagnosticReporter(context))
                .ResolveFiles();
            if (infos.Length == 0)
                return;

            if(context.ParseOptions is not CSharpParseOptions parseOptions)
            {
                context.ReportDiagnostic(
                        Diagnostic.Create(DiagnosticDescriptors.EMBED0002, Location.None));
                return;
            }

            var json = ToJson(infos);
            var gZipBase32768 = SourceFileInfoUtil.ToGZipBase32768(json);

            context.AddSource("EmbeddedSourceCode.Metadata.Generated.cs",
                SourceText.From(
                    MakeAssemblyMetadataAttributes(new Dictionary<string, string>
                    {
                        { "SourceExpander.EmbedderVersion", AssemblyUtil.AssemblyVersion.ToString() },
                        { "SourceExpander.EmbeddedSourceCode.GZipBase32768", gZipBase32768 },
                        { "SourceExpander.EmbeddedLanguageVersion", parseOptions.SpecifiedLanguageVersion.ToDisplayString()},
                    })
                , Encoding.UTF8));
        }

        static string ToJson(IEnumerable<SourceFileInfo> infos)
        {
            var serializer = new DataContractJsonSerializer(typeof(IEnumerable<SourceFileInfo>));
            using var ms = new MemoryStream();
            serializer.WriteObject(ms, infos);
            return Encoding.UTF8.GetString(ms.ToArray());
        }
        static string MakeAssemblyMetadataAttributes(IEnumerable<KeyValuePair<string, string>> dic)
        {
            var sb = new StringBuilder("using System.Reflection;");
            foreach (var p in dic)
            {
                sb.Append("[assembly: AssemblyMetadataAttribute(");
                sb.Append(p.Key.ToLiteral());
                sb.Append(",");
                sb.Append(p.Value.ToLiteral());
                sb.AppendLine(")]");
            }
            return sb.ToString();
        }
    }
}
