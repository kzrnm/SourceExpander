using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SourceExpander.Roslyn;
using System.Linq;
using System.IO;
using System;

namespace SourceExpander
{
    [Generator]
    public class EmbedderGenerator : ISourceGenerator
    {
        private const string CONFIG_FILE_NAME = "SourceExpander.Embedder.Config.json";
        public void Initialize(GeneratorInitializationContext context) { }
        public void Execute(GeneratorExecutionContext context)
        {
            var configText = context.AdditionalFiles
                .FirstOrDefault(a =>
                    StringComparer.OrdinalIgnoreCase.Compare(Path.GetFileName(a.Path), CONFIG_FILE_NAME) == 0)
                ?.GetText(context.CancellationToken);
            var config = EmbedderConfig.Parse(configText, context.CancellationToken);

            var resolver = new EmbeddingResolver(
                (CSharpCompilation)context.Compilation,
                (CSharpParseOptions)context.ParseOptions,
                new DiagnosticReporter(context),
                config,
                context.CancellationToken);

            foreach (var (path, source) in resolver.EnumerateEmbeddingSources())
            {
                context.AddSource(path, source);
            }
        }
    }
}
