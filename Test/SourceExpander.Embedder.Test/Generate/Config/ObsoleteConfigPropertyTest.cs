using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;

namespace SourceExpander.Generate.Config
{
    public class ObsoleteConfigTest : EmbedderGeneratorTestBase
    {
        public static IEnumerable<Func<(InMemorySourceText, (string Obsolete, string Instead)[])>> ObsoleteConfig_Data()
        {
            yield return () => (
                new("/foo/small/sourceExpander.embedder.config.json", """{"notmatch": 0, "enable-minify": false}"""),
                new[]
                {
                    ("enable-minify", "minify-level"),
                }
            );
            yield return () => (
                new("/foo/bar/SourceExpander.Embedder.Config.json", """{"enable-minify": true}"""),
                new[]
                {
                    ("enable-minify", "minify-level"),
                }
            );
            yield return () => (
                new("/foo/bar/SourceExpander.Embedder.Config.json", """{"embedding-source-class": {}}"""),
                new[]
                {
                    ("embedding-source-class", "embedding-source-class-name"),
                }
            );
            yield return () => (
                new("/foo/bar/SourceExpander.Embedder.Config.json", """{"expanding-symbol": "SYMBOL"}"""),
                new[]
                {
                    ("expanding-symbol", "expand-in-library"),
                }
            );
        }

        [Test]
        [MethodDataSource(nameof(ObsoleteConfig_Data))]
        public async Task ObsoleteConfig(InMemorySourceText additionalText, (string Obsolete, string Instead)[] diagnosticsArgs)
        {
            var test = new Test
            {
                TestState =
                {
                    AdditionalFiles =
                    {
                        additionalText,
                        ("/foo/bar/SourceExpander.Notmatch.json", "notmatch"),
                    },
                    Sources = {
                        ("/home/source/Program.cs", ""),
                    },
                }
            };
            foreach (var (obsolete, instead) in diagnosticsArgs)
                test.ExpectedDiagnostics.Add(
                    new DiagnosticResult("EMBED0011", DiagnosticSeverity.Warning)
                        .WithSpan(additionalText.Path, 1, 1, 1, 1)
                        .WithArguments(additionalText.Path, obsolete, instead));
            await test.RunAsync(TestContext.Current!.Execution.CancellationToken);
        }

        public static IEnumerable<Func<(DummyAnalyzerConfigOptionsProvider, (string Obsolete, string Instead)[])>> ObsoleteConfigProperty_Data()
        {
            yield return () => (
                new DummyAnalyzerConfigOptionsProvider
                {
                    { "build_property.SourceExpander_Embedder_EnableMinify", "false" },
                },
                new[]
                {
                    ("enable-minify", "minify-level"),
                }
            );
            yield return () => (
                new DummyAnalyzerConfigOptionsProvider
                {
                    { "build_property.SourceExpander_Embedder_EnableMinify", "True" },
                },
                new[]
                {
                    ("enable-minify", "minify-level"),
                }
            );
            yield return () => (
                new DummyAnalyzerConfigOptionsProvider
                {
                    { "build_property.SourceExpander_Embedder_ExpandingSymbol", "SYMBOL" },
                },
                new[]
                {
                    ("expanding-symbol", "expand-in-library"),
                }
            );
        }

        [Test]
        [MethodDataSource(nameof(ObsoleteConfigProperty_Data))]
        public async Task ObsoleteConfigProperty(DummyAnalyzerConfigOptionsProvider analyzerConfigOptionsProvider, (string Obsolete, string Instead)[] diagnosticsArgs)
        {
            var test = new Test
            {
                AnalyzerConfigOptionsProvider = analyzerConfigOptionsProvider,
                TestState =
                {
                    Sources = {
                        ("/home/source/Program.cs", ""),
                    },
                }
            };
            foreach (var (obsolete, instead) in diagnosticsArgs)
                test.ExpectedDiagnostics.Add(
                    new DiagnosticResult("EMBED0011", DiagnosticSeverity.Warning)
                    .WithArguments("Any of configs", obsolete, instead));
            await test.RunAsync(TestContext.Current!.Execution.CancellationToken);
        }
    }
}
