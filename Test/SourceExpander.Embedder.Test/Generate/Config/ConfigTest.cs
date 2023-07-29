using System.Collections.Immutable;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace SourceExpander.Generate.Config
{
    public class ConfigTest : EmbedderGeneratorTestBase
    {
        public static TheoryData ParseErrorJsons = new TheoryData<InMemorySourceText, object[]>
        {
            {
                new InMemorySourceText(
                "/foo/directory/SourceExpander.Embedder.Config.json", @"
{
    ""$schema"": ""https://raw.githubusercontent.com/kzrnm/SourceExpander/master/schema/embedder.schema.json"",
    ""embedding-type"": ""Raw"",
    ""exclude-attributes"": 1
}
"),
                new object[]
                {
                    "/foo/directory/SourceExpander.Embedder.Config.json",
                    "Error converting value 1 to type 'System.String[]'. Path 'exclude-attributes', line 5, position 27."
                }
            },
            {
                new InMemorySourceText(
                "/foo/bar/sourceExpander.embedder.config.json", @"
{
    ""$schema"": ""https://raw.githubusercontent.com/kzrnm/SourceExpander/master/schema/embedder.schema.json"",
    ""embedding-type"": ""Raw"",
    ""exclude-attributes"": 1
}
"),
                new object[]
                {
                    "/foo/bar/sourceExpander.embedder.config.json",
                    "Error converting value 1 to type 'System.String[]'. Path 'exclude-attributes', line 5, position 27."
                }
            },
        };

        [Theory]
        [MemberData(nameof(ParseErrorJsons))]
        public async Task ParseError(InMemorySourceText additionalText, object[] diagnosticsArg)
        {
            var embeddedNamespaces = ImmutableArray<string>.Empty;
            var embeddedFiles = ImmutableArray.Create(
                 new SourceFileInfo
                 (
                     "TestProject>Program.cs",
                     new string[] { "Program" },
                     ImmutableArray.Create("using System;", "using System.Diagnostics;"),
                     ImmutableArray<string>.Empty,
                     @"[DebuggerDisplay(""Name"")] class Program { static void Main() { Console.WriteLine(1); }  [System.Diagnostics.Conditional(""TEST"")] static void T() => Console.WriteLine(2); }"
                 ));
            string embeddedSourceCode = SourceFileInfoUtil.ToGZipBase32768(@"[{""CodeBody"":""[DebuggerDisplay(\""Name\"")] class Program { static void Main() { Console.WriteLine(1); }  [System.Diagnostics.Conditional(\""TEST\"")] static void T() => Console.WriteLine(2); }"",""Dependencies"":[],""FileName"":""TestProject>Program.cs"",""TypeNames"":[""Program""],""Usings"":[""using System;"",""using System.Diagnostics;""]}]");
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
                        (
                            "/home/source/Program.cs",
                            @"
using System;
using System.Diagnostics;

[DebuggerDisplay(""Name"")]
class Program
{
    static void Main()
    {
        Console.WriteLine(1);
    }

    [System.Diagnostics.Conditional(""TEST"")]
    static void T() => Console.WriteLine(2);
}
"
                        ),
                    },
                    GeneratedSources =
                    {
                        (typeof(EmbedderGenerator), "EmbeddedSourceCode.Metadata.cs",
                        $"""
                        using System.Reflection;
                        [assembly: AssemblyMetadataAttribute("SourceExpander.EmbedderVersion","{EmbedderVersion}")]
                        [assembly: AssemblyMetadataAttribute("SourceExpander.EmbeddedLanguageVersion","{EmbeddedLanguageVersion}")]
                        [assembly: AssemblyMetadataAttribute("SourceExpander.EmbeddedNamespaces","{string.Join(",", embeddedNamespaces)}")]
                        [assembly: AssemblyMetadataAttribute("SourceExpander.EmbeddedSourceCode.GZipBase32768",{embeddedSourceCode.ToLiteral()})]
                        
                        """
                        ),
                    },
                    ExpectedDiagnostics =
                    {
                        DiagnosticResult.CompilerError("EMBED0003")
                            .WithSpan(additionalText.Path, 1, 1, 1, 1)
                            .WithArguments(diagnosticsArg),
                    }
                }
            };
            await test.RunAsync();
            Newtonsoft.Json.JsonConvert.DeserializeObject<SourceFileInfo[]>(
                SourceFileInfoUtil.FromGZipBase32768(embeddedSourceCode))
                .Should()
                .BeEquivalentTo(embeddedFiles);
            System.Text.Json.JsonSerializer.Deserialize<SourceFileInfo[]>(
                SourceFileInfoUtil.FromGZipBase32768(embeddedSourceCode))
                .Should()
                .BeEquivalentTo(embeddedFiles);
        }

        [Fact]
        public async Task WithoutConfig()
        {
            var embeddedNamespaces = ImmutableArray<string>.Empty;
            var embeddedFiles = ImmutableArray.Create(
                 new SourceFileInfo
                 (
                     "TestProject>Program.cs",
                     new string[] { "Program" },
                     ImmutableArray.Create("using System;", "using System.Diagnostics;"),
                     ImmutableArray<string>.Empty,
                     @"[DebuggerDisplay(""Name"")] class Program { static void Main() { Console.WriteLine(1); }  [System.Diagnostics.Conditional(""TEST"")] static void T() => Console.WriteLine(2); }"
                 ));
            string embeddedSourceCode = SourceFileInfoUtil.ToGZipBase32768("[{\"CodeBody\":\"[DebuggerDisplay(\\\"Name\\\")] class Program { static void Main() { Console.WriteLine(1); }  [System.Diagnostics.Conditional(\\\"TEST\\\")] static void T() => Console.WriteLine(2); }\",\"Dependencies\":[],\"FileName\":\"TestProject>Program.cs\",\"TypeNames\":[\"Program\"],\"Usings\":[\"using System;\",\"using System.Diagnostics;\"]}]");

            var test = new Test
            {
                TestState =
                {
                    Sources = {
                        (
                            "/home/source/Program.cs",
                            @"
using System;
using System.Diagnostics;

[DebuggerDisplay(""Name"")]
class Program
{
    static void Main()
    {
        Console.WriteLine(1);
    }

    [System.Diagnostics.Conditional(""TEST"")]
    static void T() => Console.WriteLine(2);
}
"
                        ),
                    },
                    GeneratedSources =
                    {
                        (typeof(EmbedderGenerator), "EmbeddedSourceCode.Metadata.cs",
                        $"""
                        using System.Reflection;
                        [assembly: AssemblyMetadataAttribute("SourceExpander.EmbedderVersion","{EmbedderVersion}")]
                        [assembly: AssemblyMetadataAttribute("SourceExpander.EmbeddedLanguageVersion","{EmbeddedLanguageVersion}")]
                        [assembly: AssemblyMetadataAttribute("SourceExpander.EmbeddedNamespaces","{string.Join(",", embeddedNamespaces)}")]
                        [assembly: AssemblyMetadataAttribute("SourceExpander.EmbeddedSourceCode.GZipBase32768",{embeddedSourceCode.ToLiteral()})]
                        
                        """
                        ),
                    }
                }
            };
            await test.RunAsync();
            Newtonsoft.Json.JsonConvert.DeserializeObject<SourceFileInfo[]>(
                SourceFileInfoUtil.FromGZipBase32768(embeddedSourceCode))
                .Should()
                .BeEquivalentTo(embeddedFiles);
            System.Text.Json.JsonSerializer.Deserialize<SourceFileInfo[]>(
                SourceFileInfoUtil.FromGZipBase32768(embeddedSourceCode))
                .Should()
                .BeEquivalentTo(embeddedFiles);
        }

        [Fact]
        public async Task NotEnabled()
        {
            var additionalText = new InMemorySourceText(
                "/foo/bar/SourceExpander.Embedder.Config.json", @"
{
    ""$schema"": ""https://raw.githubusercontent.com/kzrnm/SourceExpander/master/schema/embedder.schema.json"",
    ""embedding-type"": ""Raw"",
    ""enabled"": false,
    ""exclude-attributes"": [
        ""System.Diagnostics.DebuggerDisplayAttribute""
    ]
}
");

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
                        (
                            "/home/source/Program.cs",
                            @"
using System;

class Program
{
    static void Main()
    {
        Console.WriteLine(1);
    }
}
"
                        ),
                    },
                }
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task NotEnabledProperty()
        {
            var analyzerConfigOptionsProvider = new DummyAnalyzerConfigOptionsProvider
            {
                { "build_property.SourceExpander_Embedder_Enabled", "false" },
            };

            var test = new Test
            {
                AnalyzerConfigOptionsProvider = analyzerConfigOptionsProvider,
                TestState =
                {
                    Sources = {
                        (
                            "/home/source/Program.cs",
                            """
using System;

class Program
{
    static void Main()
    {
        Console.WriteLine(1);
    }
}
"""
                        ),
                    },
                }
            };
            await test.RunAsync();
        }
    }
}
