using System.Collections.Generic;
using System.Linq;
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
            var embeddedDatas = AssemblyMetadataUtil.GetEmbeddedSourceFiles(compilation);
            container = new SourceFileContainer(WithCheck(embeddedDatas));
            expander = new CompilationExpander(compilation, container);
        }
        private IEnumerable<EmbeddedData> WithCheck(IEnumerable<EmbeddedData> embeddedDatas)
        {
            foreach (var embedded in embeddedDatas)
            {
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

        public IEnumerable<(string filePath, string expandedCode)> EnumerateExpandedCodes()
        {
            foreach (var tree in compilation.SyntaxTrees)
            {
                var filePath = tree.FilePath;
                if (!config.IgnoreFilePatterns.Any(regex => regex.IsMatch(filePath)))
                    yield return (filePath, expander.ExpandCode(tree, cancellationToken));
            }
        }

        public bool IsEmbeddedEmpty => container.Count == 0;
    }
}
