using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp;
using SourceExpander.Roslyn;

namespace SourceExpander
{
    internal class EmbeddedLoaderWithDiagnostic(
        CSharpCompilation compilation,
        CSharpParseOptions parseOptions,
        IDiagnosticReporter reporter,
        ExpandConfig config,
        CancellationToken cancellationToken = default)
        : EmbeddedLoader(
            compilation,
            parseOptions,
            config,
            ResolveEmbeddedData(compilation, parseOptions, config, reporter, cancellationToken),
            cancellationToken)
    {
        protected readonly IDiagnosticReporter reporter = reporter;

        private static SourceFileContainer ResolveEmbeddedData(CSharpCompilation compilation, CSharpParseOptions parseOptions, ExpandConfig config, IDiagnosticReporter reporter, CancellationToken cancellationToken)
        {
            var embeddedDatas = new AssemblyMetadataResolver(compilation).GetEmbeddedSourceFiles(false, cancellationToken);
            var returnDatas = new List<EmbeddedData>(embeddedDatas.Length);
            var ignoreAssemblies = new HashSet<string>(config.IgnoreAssemblies);
            foreach (var (embedded, display, errors) in embeddedDatas)
            {
                cancellationToken.ThrowIfCancellationRequested();
                foreach (var (key, message) in errors)
                {
                    reporter.ReportDiagnostic(
                        DiagnosticDescriptors.EXPAND0008_EmbeddedDataError(display, key, message));
                }
                if (embedded.IsEmpty || ignoreAssemblies.Contains(embedded.AssemblyName))
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
                returnDatas.Add(embedded);
            }
            return new SourceFileContainer(returnDatas);
        }
    }
}
