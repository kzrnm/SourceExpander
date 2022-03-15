using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SourceExpander.Roslyn;

namespace SourceExpander
{
    [Generator]
    public class EmbedderGenerator : EmbedderGeneratorBase, ISourceGenerator
    {
        private const string CONFIG_FILE_NAME = "SourceExpander.Embedder.Config.json";

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForPostInitialization(ctx =>
            {
                foreach (var (hintName, sourceText) in CompileTimeTypeMaker.Sources)
                    ctx.AddSource(hintName, sourceText);
            });
        }
        public void Execute(GeneratorExecutionContext context)
        {
            var (config, diagnostic) = ParseAdditionalTextAndAnalyzerOptions(
                context.AdditionalFiles.Where(a => StringComparer.OrdinalIgnoreCase.Compare(Path.GetFileName(a.Path), CONFIG_FILE_NAME) == 0)
                .FirstOrDefault(), context.AnalyzerConfigOptions, context.CancellationToken);
            Execute(new GeneratorExecutionContextWrapper(context), (CSharpCompilation)context.Compilation, context.ParseOptions, config, diagnostic);
        }
    }
}
