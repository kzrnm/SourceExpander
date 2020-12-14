using System.Threading;
using Microsoft.CodeAnalysis.CSharp;
using SourceExpander.Roslyn;

namespace SourceExpander
{
#pragma warning disable CA1815 // Override equals and operator equals on value types
    public readonly struct EmbeddingContext
#pragma warning restore CA1815 // Override equals and operator equals on value types
    {
        public CSharpCompilation Compilation { get; }
        public CSharpParseOptions ParseOptions { get; }
        public IDiagnosticReporter Reporter { get; }
        public EmbedderConfig Config { get; }
        public CancellationToken CancellationToken { get; }
        public EmbeddingContext(
            CSharpCompilation compilation,
            CSharpParseOptions parseOptions,
            IDiagnosticReporter reporter,
            EmbedderConfig config,
            CancellationToken cancellationToken = default)
        {
            Compilation = compilation;
            ParseOptions = parseOptions;
            Reporter = reporter;
            Config = config;
            CancellationToken = cancellationToken;
        }
    }
}
