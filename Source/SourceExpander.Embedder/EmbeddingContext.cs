using System.Threading;
using Microsoft.CodeAnalysis.CSharp;
using SourceExpander.Roslyn;

namespace SourceExpander
{
    internal readonly struct EmbeddingContext
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
