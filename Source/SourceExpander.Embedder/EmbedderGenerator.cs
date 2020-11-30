using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SourceExpander.Roslyn;

namespace SourceExpander
{
    [Generator]
    public class EmbedderGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context) { }
        public void Execute(GeneratorExecutionContext context)
        {
            var resolver = new EmbeddingResolver(
                (CSharpCompilation)context.Compilation,
                (CSharpParseOptions)context.ParseOptions,
                new DiagnosticReporter(context),
                context.CancellationToken);

            foreach (var (path, source) in resolver.EnumerateEmbeddingSources())
            {
                context.AddSource(path, source);
            }
        }
    }
}
