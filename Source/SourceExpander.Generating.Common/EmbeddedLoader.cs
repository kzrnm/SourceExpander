﻿using System;
using System.Collections.Generic;
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
        public EmbeddedLoader(
            CSharpCompilation compilation,
            CSharpParseOptions parseOptions,
            ExpandConfig config,
            CancellationToken cancellationToken = default)
            : this(compilation, parseOptions, config, ResolveEmbeddedData(compilation, cancellationToken), cancellationToken)
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

        private static SourceFileContainer ResolveEmbeddedData(CSharpCompilation compilation, CancellationToken cancellationToken)
        {
            var embeddedDatas = new AssemblyMetadataResolver(compilation).GetEmbeddedSourceFiles(false, cancellationToken);
            return new SourceFileContainer(embeddedDatas.Select(t => t.Data));
        }


        private bool compilationUpdated = false;
        private void UpdateCompilation()
        {
            if (compilationUpdated) return;
            compilationUpdated = true;

            IEnumerable<SyntaxTree> newTrees;
            if (compilation.Options.ConcurrentBuild)
                newTrees = compilation.SyntaxTrees.AsParallel(cancellationToken)
                    .Select(Rewrited);
            else
                newTrees = compilation.SyntaxTrees.Do(_ => cancellationToken.ThrowIfCancellationRequested())
                    .Select(Rewrited);
            compilation = compilation.RemoveAllSyntaxTrees().AddSyntaxTrees(newTrees);

            SyntaxTree Rewrited(SyntaxTree tree)
                => tree.WithRootAndOptions(tree.GetRoot(cancellationToken), parseOptions);
        }

        /// <summary>
        /// get expanded codes
        /// </summary>
        public ImmutableArray<(string FilePath, string expandedCode)> ExpandedCodes()
        {
            if (!config.Enabled) return ImmutableArray<(string FilePath, string expandedCode)>.Empty;
            UpdateCompilation();
            cancellationToken.ThrowIfCancellationRequested();

            return Impl();

            ImmutableArray<(string, string)> Impl()
            {
                var expander = new CompilationExpander(compilation, container, config);
                if (compilation.Options.ConcurrentBuild)
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

        /// <summary>
        /// get dependencies
        /// </summary>
        public ImmutableArray<(string FilePath, ImmutableArray<string> Dependencies)> Dependencies()
        {
            if (!config.Enabled) return ImmutableArray<(string FilePath, ImmutableArray<string> Dependencies)>.Empty;
            UpdateCompilation();
            cancellationToken.ThrowIfCancellationRequested();

            var expander = new CompilationExpander(compilation, container, config);
            (string FilePath, ImmutableArray<string> Dependencies) ResolveDependency(SyntaxTree tree)
            {
                if (expander is null)
                    throw new InvalidProgramException("expander must not be null.");
                var deps = expander.ResolveDependency(tree, cancellationToken).Select(s => s.FileName);
                return (tree.FilePath, ImmutableArray.CreateRange(deps));
            }

            if (compilation.Options.ConcurrentBuild)
                return compilation.SyntaxTrees.AsParallel(cancellationToken)
                    .Select(ResolveDependency)
                    .ToImmutableArray();
            else
                return compilation.SyntaxTrees.Do(_ => cancellationToken.ThrowIfCancellationRequested())
                    .Select(ResolveDependency)
                    .ToImmutableArray();
        }

        /// <summary>
        /// count of embedded code.
        /// </summary>
        public bool IsEmbeddedEmpty => container.Count == 0;

        public string ExpandAll(CancellationToken token) => new CompilationExpander(compilation, container, config).ExpandAll(token);
    }
}
