using System;
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
    public class ExpandGenerator : ExpandGeneratorBase, ISourceGenerator
    {
        private const string CONFIG_FILE_NAME = "SourceExpander.Generator.Config.json";

        public void Initialize(GeneratorInitializationContext context) { }
        public void Execute(GeneratorExecutionContext context)
        {
            if (context.ParseOptions is CSharpParseOptions { LanguageVersion: <= LanguageVersion.CSharp3 })
            {
                context.ReportDiagnostic(
                    DiagnosticDescriptors.EXPAND0004_MustBeNewerThanCSharp3());
            }
            const string SourceExpander_Expanded_SourceCode = "SourceExpander.Expanded.SourceCode";
            if (context.Compilation.GetTypeByMetadataName(SourceExpander_Expanded_SourceCode) is null)
            {
                context.AddSource("SourceExpander.SourceCode.cs",
                   SourceText.From(EmbeddingCore.SourceCodeClassCode, new UTF8Encoding(false)));
            }

            var (config, diagnostic) = ParseAdditionalTextAndAnalyzerOptions(
                context.AdditionalFiles.Where(a => StringComparer.OrdinalIgnoreCase.Compare(Path.GetFileName(a.Path), CONFIG_FILE_NAME) == 0)
                .FirstOrDefault(), context.AnalyzerConfigOptions, context.CancellationToken);
            Execute(new GeneratorExecutionContextWrapper(context), (CSharpCompilation)context.Compilation, (CSharpParseOptions)context.ParseOptions, config, diagnostic);
        }
    }
}
