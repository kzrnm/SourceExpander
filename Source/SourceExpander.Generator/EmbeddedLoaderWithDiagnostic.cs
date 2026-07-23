using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp;
using SourceExpander.Roslyn;

namespace SourceExpander
{
    internal class EmbeddedLoaderWithDiagnostic(
        CSharpCompilation compilation,
        CSharpParseOptions parseOptions,
        ImmutableArray<EmbeddedData> embeddeds,
        IDiagnosticReporter reporter,
        ExpandConfig config,
        CancellationToken cancellationToken = default)
        : EmbeddedLoader(
            compilation,
            parseOptions,
            config,
            ResolveEmbeddedData(compilation, parseOptions, embeddeds, config, reporter, cancellationToken),
            cancellationToken)
    {
        protected readonly IDiagnosticReporter reporter = reporter;

        private static SourceFileContainer ResolveEmbeddedData(
            CSharpCompilation compilation,
            CSharpParseOptions parseOptions,
            ImmutableArray<EmbeddedData> embeddeds,
            ExpandConfig config,
            IDiagnosticReporter reporter,
            CancellationToken cancellationToken)
        {
            var embeddedDatas = new AssemblyMetadataResolver(compilation).GetEmbeddedSourceFiles(embeddeds, false, cancellationToken);
            var returnDatas = new List<EmbeddedData>(embeddeds.Length + embeddedDatas.Length);
            var ignoreAssemblies = new HashSet<string>(config.IgnoreAssemblies);
            foreach (var (embedded, display, errors) in embeddedDatas)
            {
                cancellationToken.ThrowIfCancellationRequested();
                foreach (var (key, message) in errors)
                {
                    reporter.ReportDiagnostic(
                        DiagnosticDescriptors.EXPAND0008_EmbeddedDataError(display, key, message));
                }
                if (ignoreAssemblies.Contains(embedded.AssemblyName))
                    continue;
                if (embedded.EmbedderVersion > AssemblyUtil.AssemblyVersion)
                {
                    reporter.ReportDiagnostic(
                        DiagnosticDescriptors.EXPAND0002_ExpanderVersion(
                        AssemblyUtil.AssemblyVersion, embedded.AssemblyName, embedded.EmbedderVersion));
                }
                if (embedded.Sources.IsEmpty)
                    continue;

                LanguageVersionFacts.TryParse(embedded.CSharpVersion, out var csharpVersion);
                if (csharpVersion > parseOptions.LanguageVersion)
                {
                    reporter.ReportDiagnostic(
                        DiagnosticDescriptors.EXPAND0005_NewerCSharpVersion(
                        parseOptions.LanguageVersion, embedded.AssemblyName, csharpVersion));
                }
                returnDatas.Add(embedded);
            }
            return new SourceFileContainer(returnDatas);
        }
    }
}
