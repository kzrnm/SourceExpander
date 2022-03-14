using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SourceExpander.Roslyn;

namespace SourceExpander
{
    [Generator]
    public class EmbedderGenerator : EmbedderGeneratorBase, IIncrementalGenerator
    {
        private const string CONFIG_FILE_NAME = "SourceExpander.Embedder.Config.json";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterPostInitializationOutput(ctx =>
            {
                foreach (var (hintName, sourceText) in CompileTimeTypeMaker.Sources)
                    ctx.AddSource(hintName, sourceText);
            });

            IncrementalValueProvider<(EmbedderConfig Config, ImmutableArray<Diagnostic> Diagnostic)> configProvider
                = context.AdditionalTextsProvider
                .Where(a => StringComparer.OrdinalIgnoreCase.Compare(Path.GetFileName(a.Path), CONFIG_FILE_NAME) == 0)
                .Collect()
                .Select((ats, _) => ats.FirstOrDefault())
                .Combine(context.AnalyzerConfigOptionsProvider)
                .Select((tup, ct) => ParseAdditionalTextAndAnalyzerOptions(tup.Left, tup.Right, ct));


            var source = context.CompilationProvider
                .Combine(context.ParseOptionsProvider)
                .Combine(configProvider);

            context.RegisterImplementationSourceOutput(source, Execute);
        }

        private void Execute(SourceProductionContext ctx, ((Compilation Left, ParseOptions Right) Left, (EmbedderConfig Config, ImmutableArray<Diagnostic> Diagnostic) Right) source)
        {
            var ((compilation, parseOptions), (config, configDiagnostic)) = source;
            Execute(new SourceProductionContextWrappter(ctx), (CSharpCompilation)compilation, parseOptions, config, configDiagnostic);
        }
    }
}
