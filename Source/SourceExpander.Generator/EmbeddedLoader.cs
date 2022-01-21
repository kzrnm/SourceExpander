using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace SourceExpander
{
    internal class EmbeddedLoader
    {
        protected CSharpCompilation compilation;
        protected readonly CSharpParseOptions parseOptions;
        protected readonly SourceFileContainer container;
        protected readonly ExpandConfig config;
        protected readonly bool ConcurrentBuild;
        protected readonly CancellationToken cancellationToken;

        public EmbeddedLoader(
            CSharpCompilation compilation,
            CSharpParseOptions parseOptions,
            ExpandConfig config,
            CancellationToken cancellationToken = default)
            : this(compilation, parseOptions, config, ResolveEmbeddedData(compilation, cancellationToken), cancellationToken)
        {
        }

        public EmbeddedLoader(
            CSharpCompilation compilation,
            CSharpParseOptions parseOptions,
            ExpandConfig config,
            SourceFileContainer container,
            CancellationToken cancellationToken = default)
        {
            this.compilation = compilation;
            this.ConcurrentBuild = compilation.Options.ConcurrentBuild;
            this.parseOptions = parseOptions.WithDocumentationMode(DocumentationMode.Diagnose);
            this.config = config;
            this.cancellationToken = cancellationToken;
            this.container = container;
        }

        private static SourceFileContainer ResolveEmbeddedData(CSharpCompilation compilation, CancellationToken cancellationToken)
        {
            var embeddedDatas = new AssemblyMetadataResolver(compilation).GetEmbeddedSourceFiles(cancellationToken);
            return new SourceFileContainer(embeddedDatas.Select(t => t.Data));
        }


        bool compilationUpdated = false;
        void UpdateCompilation()
        {
            if (compilationUpdated) return;
            compilationUpdated = true;

            IEnumerable<SyntaxTree> newTrees;
            if (ConcurrentBuild)
                newTrees = compilation.SyntaxTrees.AsParallel(cancellationToken)
                    .Select(Rewrited);
            else
                newTrees = compilation.SyntaxTrees.Do(_ => cancellationToken.ThrowIfCancellationRequested())
                    .Select(Rewrited);
            compilation = compilation.RemoveAllSyntaxTrees().AddSyntaxTrees(newTrees);

            SyntaxTree Rewrited(SyntaxTree tree)
                => tree.WithRootAndOptions(tree.GetRoot(cancellationToken), parseOptions);
        }

        private ImmutableArray<(string filePath, string expandedCode)> _cacheExpandedCodes;

        public ImmutableArray<(string filePath, string expandedCode)> ExpandedCodes()
        {
            if (!_cacheExpandedCodes.IsDefault) return _cacheExpandedCodes;
            if (!config.Enabled) return ImmutableArray<(string filePath, string expandedCode)>.Empty;
            UpdateCompilation();
            cancellationToken.ThrowIfCancellationRequested();

            return _cacheExpandedCodes = Impl();

            ImmutableArray<(string, string)> Impl()
            {
                var expander = new CompilationExpander(compilation, container, config);
                if (ConcurrentBuild)
                    return compilation.SyntaxTrees.AsParallel(cancellationToken)
                        .Where(tree => config.IsMatch(tree.FilePath))
                        .Select(tree => (tree.FilePath, expander.ExpandCode(tree, cancellationToken)))
                        .OrderBy(tree => tree.FilePath, StringComparer.Ordinal)
                        .ToImmutableArray();
                else
                    return compilation.SyntaxTrees.Do(_ => cancellationToken.ThrowIfCancellationRequested())
                        .Where(tree => config.IsMatch(tree.FilePath))
                        .Select(tree => (tree.FilePath, expander.ExpandCode(tree, cancellationToken)))
                        .OrderBy(tree => tree.FilePath, StringComparer.Ordinal)
                        .ToImmutableArray();
            }
        }

        public bool IsEmbeddedEmpty => container.Count == 0;
    }
}
