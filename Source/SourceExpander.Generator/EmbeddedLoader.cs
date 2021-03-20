using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SourceExpander.Roslyn;

namespace SourceExpander
{
    public class EmbeddedLoader
    {
        private CSharpCompilation compilation;
        private readonly CSharpParseOptions parseOptions;
        private readonly SourceFileContainer container;
        private readonly IDiagnosticReporter reporter;
        private readonly ExpandConfig config;
        private readonly bool ConcurrentBuild;
        private readonly CancellationToken cancellationToken;

        public EmbeddedLoader(
            CSharpCompilation compilation,
            CSharpParseOptions parseOptions,
            IDiagnosticReporter reporter,
            ExpandConfig config,
            CancellationToken cancellationToken = default)
        {
            this.reporter = reporter;
            this.compilation = compilation;
            this.ConcurrentBuild = compilation.Options.ConcurrentBuild;
            this.parseOptions = parseOptions.WithDocumentationMode(DocumentationMode.Diagnose);
            this.config = config;
            this.cancellationToken = cancellationToken;
            var embeddedDatas = new AssemblyMetadataResolver(compilation).GetEmbeddedSourceFiles(cancellationToken);
            container = new SourceFileContainer(WithCheck(embeddedDatas));
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

        public bool IsEmbeddedEmpty => container.Count == 0;

        private IEnumerable<EmbeddedData> WithCheck(IEnumerable<(EmbeddedData Data, string? Display, ImmutableArray<(string Key, string ErrorMessage)> Errors)> embeddedDatas)
        {
            foreach (var (embedded, display, errors) in embeddedDatas)
            {
                foreach (var (key, message) in errors)
                {
                    reporter.ReportDiagnostic(
                        DiagnosticDescriptors.EXPAND0008_EmbeddedDataError(display, key, message));
                }
                if (embedded.IsEmpty)
                    continue;
                if (embedded.EmbedderVersion > AssemblyUtil.AssemblyVersion)
                {
                    reporter.ReportDiagnostic(
                        DiagnosticDescriptors.EXPAND0002_ExpanderVersion(
                        AssemblyUtil.AssemblyVersion, embedded.AssemblyName, embedded.EmbedderVersion));
                }
                if (embedded.CSharpVersion > parseOptions.LanguageVersion)
                {
                    reporter.ReportDiagnostic(
                        DiagnosticDescriptors.EXPAND0005_NewerCSharpVersion(
                        parseOptions.LanguageVersion, embedded.AssemblyName, embedded.CSharpVersion));
                }
                if (embedded.AllowUnsafe && !compilation.Options.AllowUnsafe)
                {
                    reporter.ReportDiagnostic(
                        DiagnosticDescriptors.EXPAND0006_AllowUnsafe(
                        embedded.AssemblyName));
                }
                yield return embedded;
            }
        }
    }
}
