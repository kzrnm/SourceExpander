using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using SourceExpander.Roslyn;

namespace SourceExpander;

[Generator]
public partial class ExpandGenerator : IIncrementalGenerator
{
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

        var embeddedDataJsonsProvider = context.AdditionalTextsProvider
            .Where(a => a.Path.EndsWith(EMBEDDED_FILE_NAME, StringComparison.OrdinalIgnoreCase))
            .Collect();

        var configProvider
            = context.AdditionalTextsProvider
            .Where(a => StringComparer.OrdinalIgnoreCase.Compare(Path.GetFileName(a.Path), CONFIG_FILE_NAME) == 0)
            .Collect()
            .Select((ats, _) => ats.FirstOrDefault())
            .Combine(context.AnalyzerConfigOptionsProvider.Select((s, _) => s.GlobalOptions))
            .Select((tup, ct) => (tup.Right, ExpandConfig.LoadBuilder(tup.Left, tup.Right)));

        var source = context.CompilationProvider
            .Combine(context.ParseOptionsProvider)
            .Combine(embeddedDataJsonsProvider)
            .Combine(configProvider);

        context.RegisterImplementationSourceOutput(source, Execute);
    }

    private void Execute(SourceProductionContext ctx, (((Compilation, ParseOptions), ImmutableArray<AdditionalText>), (AnalyzerConfigOptions, ExpandConfig.Builder)) source)
    {
        var (((compilation, parseOptions), emmbeddedDataJsons), (analyzerConfigOptions, configBuilder)) = source;
        Execute(
            new SourceProductionContextWrappter(ctx),
            (CSharpCompilation)compilation,
            (CSharpParseOptions)parseOptions,
            analyzerConfigOptions,
            emmbeddedDataJsons,
            configBuilder);
    }
}
