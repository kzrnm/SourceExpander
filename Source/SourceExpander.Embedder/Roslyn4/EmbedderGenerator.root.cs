using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using SourceExpander.Roslyn;

namespace SourceExpander;

[Generator]
public partial class EmbedderGenerator : IIncrementalGenerator
{
    private const string CONFIG_FILE_NAME = "SourceExpander.Embedder.Config.json";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx =>
        {
            foreach (var (hintName, sourceText) in Constants.CompileTimeSources)
                ctx.AddSource(hintName, sourceText);
        });

        var configProvider
            = context.AdditionalTextsProvider
            .Where(a => StringComparer.OrdinalIgnoreCase.Compare(Path.GetFileName(a.Path), CONFIG_FILE_NAME) == 0)
            .Collect()
            .Select((ats, _) => ats.FirstOrDefault())
            .Combine(context.AnalyzerConfigOptionsProvider.Select((s, _) => s.GlobalOptions))
            .Select((tup, ct) => (tup.Right, EmbedderConfig.LoadBuilder(tup.Left, tup.Right)));

        var source = context.CompilationProvider
            .Combine(context.ParseOptionsProvider)
            .Combine(configProvider);

        context.RegisterImplementationSourceOutput(source, Execute);
    }

    private void Execute(SourceProductionContext ctx, ((Compilation, ParseOptions), (AnalyzerConfigOptions, EmbedderConfig.Builder)) source)
    {
        var ((compilation, parseOptions), (analyzerConfigOptions, configBuilder)) = source;
        Execute(new SourceProductionContextWrappter(ctx), (CSharpCompilation)compilation, (CSharpParseOptions)parseOptions, analyzerConfigOptions, configBuilder);
    }
}
