using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace SourceExpander
{
    /// <summary>
    /// Resolve Embedded source code
    /// </summary>
    public class EmbeddedLoader
    {
        private protected CSharpCompilation compilation;
        private protected readonly CSharpParseOptions parseOptions;
        private protected readonly SourceFileContainer container;
        private protected readonly ExpandConfig config;
        private protected readonly CancellationToken cancellationToken;

        /// <summary>
        /// constructor
        /// </summary>
        internal EmbeddedLoader(
            CSharpCompilation compilation,
            CSharpParseOptions parseOptions,
            ImmutableArray<EmbeddedData> embeddeds,
            ExpandConfig config,
            CancellationToken cancellationToken = default)
            : this(compilation, parseOptions, config, ResolveEmbeddedData(compilation, embeddeds, cancellationToken), cancellationToken)
        {
        }

        internal EmbeddedLoader(
            CSharpCompilation compilation,
            CSharpParseOptions parseOptions,
            ExpandConfig config,
            SourceFileContainer container,
            CancellationToken cancellationToken = default)
        {
            this.compilation = compilation;
            this.parseOptions = parseOptions.WithDocumentationMode(DocumentationMode.Diagnose);
            this.config = config;
            this.cancellationToken = cancellationToken;
            this.container = container;
        }

        private static SourceFileContainer ResolveEmbeddedData(CSharpCompilation compilation, ImmutableArray<EmbeddedData> embeddeds, CancellationToken cancellationToken)
        {
            var embeddedDatas = new AssemblyMetadataResolver(compilation).GetEmbeddedSourceFiles(embeddeds, false, cancellationToken);
            return new SourceFileContainer(embeddedDatas.Select(t => t.Data));
        }


        private bool compilationUpdated = false;
        private void UpdateCompilation()
        {
            if (compilationUpdated) return;
            compilationUpdated = true;

            var newTrees = compilation.SyntaxTrees
                .TryParallel(compilation.Options.ConcurrentBuild, cancellationToken)
                .Select(Rewrited);
            compilation = compilation.RemoveAllSyntaxTrees().AddSyntaxTrees(newTrees);

            SyntaxTree Rewrited(SyntaxTree tree)
                => tree.WithRootAndOptions(tree.GetRoot(cancellationToken), parseOptions);
        }

        /// <summary>
        /// get expanded codes
        /// </summary>
        public ImmutableArray<ExpandedResult> ExpandedCodes()
        {
            if (!config.Enabled) return ImmutableArray<ExpandedResult>.Empty;
            UpdateCompilation();
            cancellationToken.ThrowIfCancellationRequested();

            return Impl();

            ImmutableArray<ExpandedResult> Impl()
            {
                var expander = new CompilationExpander(compilation, container, config);
                return compilation.SyntaxTrees.TryParallel(compilation.Options.ConcurrentBuild, cancellationToken)
                    .Where(tree => config.IsMatch(tree.FilePath))
                    .Select(tree => new ExpandedResult(tree, expander.ExpandCode(tree, cancellationToken)))
                    .OrderBy(rt => rt.SyntaxTree.FilePath, StringComparer.Ordinal)
                    .ToImmutableArray();
            }
        }

        /// <summary>
        /// get dependencies
        /// </summary>
        public ImmutableArray<ResolvedDependencies> Dependencies()
        {
            if (!config.Enabled) return ImmutableArray<ResolvedDependencies>.Empty;
            UpdateCompilation();
            cancellationToken.ThrowIfCancellationRequested();

            var expander = new CompilationExpander(compilation, container, config);
            ResolvedDependencies ResolveDependency(SyntaxTree tree)
            {
                if (expander is null)
                    throw new InvalidProgramException("expander must not be null.");
                var deps = expander.ResolveDependency(tree, cancellationToken).Select(s => s.FileName);
                return new(tree.FilePath, ImmutableArray.CreateRange(deps));
            }

            return compilation.SyntaxTrees.TryParallel(compilation.Options.ConcurrentBuild, cancellationToken)
                .Select(ResolveDependency)
                .ToImmutableArray();
        }

        /// <summary>
        /// count of embedded code.
        /// </summary>
        public bool IsEmbeddedEmpty => container.Count == 0;

        /// <summary>
        /// Expand all source for testing.
        /// </summary>
        public string ExpandAllForTesting(CancellationToken token)
            => new CompilationExpander(compilation, container, config).ExpandAllForTesting(token);
    }

    /// <summary>
    /// Result of Expanding
    /// </summary>
    public record struct ExpandedResult(SyntaxTree SyntaxTree, string ExpandedCode);
    /// <summary>
    /// Dependencies
    /// </summary>
    public record struct ResolvedDependencies(string FilePath, ImmutableArray<string> Dependencies);
}
