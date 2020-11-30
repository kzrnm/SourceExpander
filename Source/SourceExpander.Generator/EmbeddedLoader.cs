using System.Collections.Generic;
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

        public EmbeddedLoader(
            CSharpCompilation compilation, 
            CSharpParseOptions parseOptions, 
            IDiagnosticReporter reporter)
        {
            var trees = compilation.SyntaxTrees;
            foreach (var tree in trees)
            {
                var newOpts = tree.Options.WithDocumentationMode(DocumentationMode.Diagnose);
                compilation = compilation.ReplaceSyntaxTree(tree, tree.WithRootAndOptions(tree.GetRoot(), newOpts));
            }

            this.reporter = reporter;
            this.compilation = compilation;
            this.parseOptions = parseOptions;
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
                yield return embedded;
            }
        }

        public IEnumerable<(string filePath, string expandedCode)> EnumerateExpandedCodes()
        {
            foreach (var tree in compilation.SyntaxTrees)
                yield return (tree.FilePath, expander.ExpandCode(tree));
        }

        public bool IsEmbeddedEmpty => container.Count == 0;
    }
}
