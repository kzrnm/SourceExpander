using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;

namespace SourceExpander.Generate.Config
{
    public class ObsoleteConfigTest : EmbedderGeneratorTestBase
    {
        public static TheoryData<InMemorySourceText, (string Obsolete, string Instead)[]> ObsoleteConfig_Data = new()
        {
            {
                new("/foo/small/sourceExpander.embedder.config.json", @"{""notmatch"": 0, ""enable-minify"": false}"),
                new[]
                {
                    ("enable-minify", "minify-level"),
                }
            },
            {
                new("/foo/bar/SourceExpander.Embedder.Config.json", @"{""enable-minify"": true}"),
                new[]
                {
                    ("enable-minify", "minify-level"),
                }
            },
            {
                new("/foo/bar/SourceExpander.Embedder.Config.json", @"{""embedding-source-class"": {}}"),
                new[]
                {
                    ("embedding-source-class", "embedding-source-class-name"),
                }
            },
            {
                new("/foo/bar/SourceExpander.Embedder.Config.json", @"{""expanding-symbol"": ""SYMBOL""}"),
                new[]
                {
                    ("expanding-symbol", "expand-in-library"),
                }
            },
        };

        [Theory]
        [MemberData(nameof(ObsoleteConfig_Data))]
        public async Task ObsoleteConfig(InMemorySourceText additionalText, (string Obsolete, string Instead)[] diagnosticsArgs)
        {
            var test = new Test
            {
                TestState =
                {
                    AdditionalFiles =
                    {
                        additionalText,
                        new InMemorySourceText("/foo/bar/SourceExpander.Notmatch.json", "notmatch"),
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
            await test.RunAsync();
        }

        public static TheoryData<DummyAnalyzerConfigOptionsProvider, (string Obsolete, string Instead)[]> ObsoleteConfigProperty_Data = new()
        {
            {
                new DummyAnalyzerConfigOptionsProvider
                {
                    { "build_property.SourceExpander_Embedder_EnableMinify", "false" },
                },
                new[]
                {
                    ("enable-minify", "minify-level"),
                }
            },
            {
                new DummyAnalyzerConfigOptionsProvider
                {
                    { "build_property.SourceExpander_Embedder_EnableMinify", "True" },
                },
                new[]
                {
                    ("enable-minify", "minify-level"),
                }
            },
            {
                new DummyAnalyzerConfigOptionsProvider
                {
                    { "build_property.SourceExpander_Embedder_ExpandingSymbol", "SYMBOL" },
                },
                new[]
                {
                    ("expanding-symbol", "expand-in-library"),
                }
            },
        };

        [Theory]
        [MemberData(nameof(ObsoleteConfigProperty_Data))]
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
            await test.RunAsync();
        }
    }
}
