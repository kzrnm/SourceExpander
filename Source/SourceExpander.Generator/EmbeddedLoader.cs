using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SourceExpander.Roslyn;

namespace SourceExpander
{
    public class EmbeddedLoader
    {
        private readonly CSharpCompilation compilation;
        private readonly CSharpParseOptions parseOptions;
        private readonly SourceFileContainer container;
        private readonly CompilationExpander expander;
        private readonly IDiagnosticReporter reporter;
        private readonly ExpandConfig config;
        private readonly CancellationToken cancellationToken;

        public EmbeddedLoader(
            CSharpCompilation compilation,
            CSharpParseOptions parseOptions,
            IDiagnosticReporter reporter,
            ExpandConfig config,
            CancellationToken cancellationToken = default)
        {
            var trees = compilation.SyntaxTrees;
            foreach (var tree in trees)
            {
                var newOpts = tree.Options.WithDocumentationMode(DocumentationMode.Diagnose);
                compilation = compilation.ReplaceSyntaxTree(tree, tree.WithRootAndOptions(tree.GetRoot(cancellationToken), newOpts));
            }

            this.reporter = reporter;
            this.compilation = compilation;
            this.parseOptions = parseOptions;
            this.config = config;
            this.cancellationToken = cancellationToken;
            var embeddedDatas = new AssemblyMetadataResolver(compilation).GetEmbeddedSourceFiles();
            container = new SourceFileContainer(WithCheck(embeddedDatas));
            expander = new CompilationExpander(compilation, container, config);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types")]
        public IEnumerable<(string filePath, string expandedCode)> EnumerateExpandedCodes()
        {
            if (!config.Enabled)
                yield break;
            foreach (var tree in compilation.SyntaxTrees)
            {
                var filePath = tree.FilePath;
                if (config.IsMatch(filePath))
                {
                    string expanded;
                    try
                    {
                        expanded = expander.ExpandCode(tree, cancellationToken);
                    }
                    catch
                    {
                        Trace.WriteLine($"failed: {filePath}");
                        continue;
                    }
                    yield return (filePath, expanded);
                }
            }
        }

        public bool IsEmbeddedEmpty => container.Count == 0;

        private IEnumerable<EmbeddedData> WithCheck(IEnumerable<(EmbeddedData Data, string? Display, ImmutableArray<(string Key, string ErrorMessage)> Errors)> embeddedDatas)
        {
            foreach (var (embedded, display, errors) in embeddedDatas)
            {
                foreach (var (key, message) in errors)
                {
                    reporter.ReportDiagnostic(
                        Diagnostic.Create(DiagnosticDescriptors.EXPAND0008_EmbeddedDataError, Location.None,
                        display, key, message));
                }
                if (embedded.IsEmpty) 
                    continue;
                if (embedded.EmbedderVersion > AssemblyUtil.AssemblyVersion)
                {
                    reporter.ReportDiagnostic(
                        Diagnostic.Create(DiagnosticDescriptors.EXPAND0002_ExpanderVersion, Location.None,
                        AssemblyUtil.AssemblyVersion, embedded.AssemblyName, embedded.EmbedderVersion));
                }
                if (embedded.CSharpVersion > parseOptions.LanguageVersion)
                {
                    reporter.ReportDiagnostic(
                        Diagnostic.Create(DiagnosticDescriptors.EXPAND0005_NewerCSharpVersion, Location.None,
                        parseOptions.LanguageVersion.ToDisplayString(), embedded.AssemblyName, embedded.CSharpVersion.ToDisplayString()));
                }
                if (embedded.AllowUnsafe && !compilation.Options.AllowUnsafe)
                {
                    reporter.ReportDiagnostic(
                        Diagnostic.Create(DiagnosticDescriptors.EXPAND0006_AllowUnsafe, Location.None,
                        embedded.AssemblyName));
                }
                yield return embedded;
            }
        }
    }
}
