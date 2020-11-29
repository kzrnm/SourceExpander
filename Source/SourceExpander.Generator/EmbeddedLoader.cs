using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SourceExpander.Roslyn;

namespace SourceExpander
{
    public class EmbeddedLoader
    {
        private readonly CSharpCompilation compilation;
        private readonly SourceFileContainer container;
        private readonly CompilationExpander expander;
        private readonly IDiagnosticReporter reporter;

        public EmbeddedLoader(CSharpCompilation compilation, IDiagnosticReporter reporter)
        {
            var trees = compilation.SyntaxTrees;
            foreach (var tree in trees)
            {
                var opts = tree.Options.WithDocumentationMode(DocumentationMode.Diagnose);
                compilation = compilation.ReplaceSyntaxTree(tree, tree.WithRootAndOptions(tree.GetRoot(), opts));
            }

            this.reporter = reporter;
            this.compilation = compilation;
            var embeddedDatas = AssemblyMetadataUtil.GetEmbeddedSourceFiles(compilation);
            container = new SourceFileContainer(WithVersionCheck(embeddedDatas));
            expander = new CompilationExpander(compilation, container);
        }
        private IEnumerable<EmbeddedData> WithVersionCheck(IEnumerable<EmbeddedData> embeddedDatas)
        {
            foreach (var embedded in embeddedDatas)
            {
                if (embedded.EmbedderVersion > AssemblyUtil.AssemblyVersion)
                {
                    reporter.ReportDiagnostic(
                        Diagnostic.Create(DiagnosticDescriptors.EXPAND0002, Location.None,
                        AssemblyUtil.AssemblyVersion, embedded.AssemblyName, embedded.EmbedderVersion));
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
