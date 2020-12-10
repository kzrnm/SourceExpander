using System;
using System.Collections.Generic;
using System.IO;
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
        private const string CONFIG_FILE_NAME = "SourceExpander.Generator.Config.json";
        public void Execute(GeneratorExecutionContext context)
        {
            if (context.Compilation is not CSharpCompilation compilation
                || context.ParseOptions is not CSharpParseOptions opts)
            {
                context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.EXPAND0003_MustBeCSharp,
                    Location.None, context.ParseOptions.Language));
                return;
            }

            if ((int)opts.LanguageVersion <= (int)LanguageVersion.CSharp3)
            {
                context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.EXPAND0004_MustBeNewerThanCSharp3,
                    Location.None, opts.LanguageVersion.ToDisplayString()));
                return;
            }

            var configText = context.AdditionalFiles
                    .FirstOrDefault(a =>
                        StringComparer.OrdinalIgnoreCase.Compare(Path.GetFileName(a.Path), CONFIG_FILE_NAME) == 0)
                    ?.GetText(context.CancellationToken);

            ExpandConfig config;
            try
            {
                config = ExpandConfig.Parse(configText, context.CancellationToken);
            }
            catch (ParseConfigException e)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.EXPAND0007_ParseConfigError, Location.None, e.Message));
                return;
            }

            var loader = new EmbeddedLoader(compilation, opts, new DiagnosticReporter(context), config);
            if (loader.IsEmbeddedEmpty)
                context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.EXPAND0001_NotFoundEmbedded, Location.None));


            if (!HasCoreReference(context.Compilation.ReferencedAssemblyNames))
            {
                context.AddSource("SourceExpander.SourceCode.cs",
                   SourceText.From(EmbeddingCore.SourceCodeClassCode, Encoding.UTF8));
            }

            var expandedCode = CreateExpanded(loader);

            context.AddSource("SourceExpander.Expanded.cs",
                SourceText.From(expandedCode, Encoding.UTF8));
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
            sb.AppendLine("public static IReadOnlyDictionary<string, SourceCode> Files {get{ return _Files; }}");
            sb.AppendLine("private static Dictionary<string, SourceCode> _Files = new Dictionary<string, SourceCode>{");
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
