using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp;
using SourceExpander.Roslyn;

namespace SourceExpander
{
    internal class EmbeddedLoaderWithDiagnostic : EmbeddedLoader
    {
        protected readonly IDiagnosticReporter reporter;
        public EmbeddedLoaderWithDiagnostic(
            CSharpCompilation compilation,
            CSharpParseOptions parseOptions,
            IDiagnosticReporter reporter,
            ExpandConfig config,
            CancellationToken cancellationToken = default)
            : base(compilation, parseOptions, config,
                  ResolveEmbeddedData(compilation, parseOptions, reporter, cancellationToken),
                  cancellationToken)
        {
            this.reporter = reporter;
        }

        private static SourceFileContainer ResolveEmbeddedData(CSharpCompilation compilation, CSharpParseOptions parseOptions, IDiagnosticReporter reporter, CancellationToken cancellationToken)
        {
            var embeddedDatas = new AssemblyMetadataResolver(compilation).GetEmbeddedSourceFiles(false, cancellationToken);
            var returnDatas = new List<EmbeddedData>(embeddedDatas.Length);
            foreach (var (embedded, display, errors) in embeddedDatas)
            {
                cancellationToken.ThrowIfCancellationRequested();
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
                returnDatas.Add(embedded);
            }
            return new SourceFileContainer(returnDatas);
        }
    }
}
