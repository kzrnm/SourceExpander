using System;
using System.Collections.Immutable;
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
    public class ExpandGenerator : ExpandGeneratorBase, IIncrementalGenerator
    {
        private const string CONFIG_FILE_NAME = "SourceExpander.Generator.Config.json";
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterSourceOutput(context.ParseOptionsProvider, (ctx, opts) =>
            {
                if ((CSharpParseOptions)opts is { LanguageVersion: <= LanguageVersion.CSharp3 })
                {
                    ctx.ReportDiagnostic(
                        DiagnosticDescriptors.EXPAND0004_MustBeNewerThanCSharp3());
                }
            });

            context.RegisterSourceOutput(context.CompilationProvider, (ctx, compilation) =>
            {
                const string SourceExpander_Expanded_SourceCode = "SourceExpander.Expanded.SourceCode";
                if (compilation.GetTypeByMetadataName(SourceExpander_Expanded_SourceCode) is null)
                {
                    ctx.AddSource("SourceExpander.SourceCode.cs",
                       SourceText.From(EmbeddingCore.SourceCodeClassCode, new UTF8Encoding(false)));
                }
            });

            IncrementalValueProvider<(ExpandConfig Config, ImmutableArray<Diagnostic> Diagnostic)> configProvider
                = context.AdditionalTextsProvider
                .Where(a => StringComparer.OrdinalIgnoreCase.Compare(Path.GetFileName(a.Path), CONFIG_FILE_NAME) == 0)
                .Collect()
                .Select((ats, _) => ats.FirstOrDefault())
                .Combine(context.AnalyzerConfigOptionsProvider)
                .Select((tup, ct) => ParseAdditionalTexts(tup.Left, tup.Right, ct));

            var source = context.CompilationProvider
                .Combine(context.ParseOptionsProvider)
                .Combine(configProvider);

            context.RegisterImplementationSourceOutput(source, Execute);
        }

        private void Execute(SourceProductionContext ctx, ((Compilation Left, ParseOptions Right) Left, (ExpandConfig Config, ImmutableArray<Diagnostic> Diagnostic) Right) source)
        {
            var ((compilation, parseOptions), (config, configDiagnostic)) = source;
            Execute(new SourceProductionContextWrappter(ctx), (CSharpCompilation)compilation, parseOptions, config, configDiagnostic);
        }
    }
}
