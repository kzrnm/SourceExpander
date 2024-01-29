using System.Collections.Immutable;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace SourceExpander.Generate.Config
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1861", Justification = "For test")]
    public class IncludeExcludeTest : EmbedderGeneratorTestBase
    {
        public static TheoryData<string[]> Include_Data => new()
        {
            new []{ "/home/**" },
            new []{ "/home/**/*.cs" },
            new []{ "/home/source/Program.cs" },
        };

        [Theory]
        [MemberData(nameof(Include_Data))]
        public async Task Include(string[] data)
        {
            var additionalText = new InMemorySourceText(
                "/foo/bar/SourceExpander.Embedder.Config.json", $$$"""
{
    "$schema": "https://raw.githubusercontent.com/kzrnm/SourceExpander/master/schema/embedder.schema.json",
    "embedding-type": "Raw",
    "include": {{{data}}},
    "minify-level": "full"
}
""");

            var embeddedNamespaces = ImmutableArray<string>.Empty;
            var embeddedFiles = ImmutableArray.Create(
                 new SourceFileInfo
                 (
                     "TestProject>Program.cs",
                     ["Program"],
                     ImmutableArray.Create("using System;"),
                     ImmutableArray<string>.Empty,
                     @"class Program{static void Main(){Console.WriteLine(1);}[System.Diagnostics.Conditional(""TEST"")]static void T()=>Console.WriteLine(2);}"
                 ));
            const string embeddedSourceCode = "[{\"CodeBody\":\"class Program{static void Main(){Console.WriteLine(1);}[System.Diagnostics.Conditional(\\\"TEST\\\")]static void T()=>Console.WriteLine(2);}\",\"Dependencies\":[],\"FileName\":\"TestProject>Program.cs\",\"TypeNames\":[\"Program\"],\"Usings\":[\"using System;\"]}]";

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
                            "/other/Dummy.cs",
                            @"class Dummy{}"
                        ),
                        (
                            "/other/Dummy2.cs",
                            @"class Dummy2{}"
                        ),
                        (
                            "/home/source/Program.cs",
                            @"
using System;
using System.Diagnostics;

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
                        (typeof(EmbedderGenerator), "EmbeddedSourceCode.Metadata.cs",$"""
                        using System.Reflection;
                        [assembly: AssemblyMetadataAttribute("SourceExpander.EmbedderVersion","{EmbedderVersion}")]
                        [assembly: AssemblyMetadataAttribute("SourceExpander.EmbeddedLanguageVersion","{EmbeddedLanguageVersion}")]
                        [assembly: AssemblyMetadataAttribute("SourceExpander.EmbeddedNamespaces","{string.Join(",", embeddedNamespaces)}")]
                        [assembly: AssemblyMetadataAttribute("SourceExpander.EmbeddedSourceCode",{embeddedSourceCode.ToLiteral()})]
                        
                        """
                        ),
                    }
                }
            };
            await test.RunAsync();
            Newtonsoft.Json.JsonConvert.DeserializeObject<SourceFileInfo[]>(embeddedSourceCode)
                .Should()
                .BeEquivalentTo(embeddedFiles);
            System.Text.Json.JsonSerializer.Deserialize<SourceFileInfo[]>(embeddedSourceCode)
                .Should()
                .BeEquivalentTo(embeddedFiles);
        }

        [Fact]
        public async Task IncludeProperty()
        {
            var analyzerConfigOptionsProvider = new DummyAnalyzerConfigOptionsProvider
            {
                { "build_property.SourceExpander_Embedder_EmbeddingType", "raw" },
                { "build_property.SourceExpander_Embedder_MinifyLevel", "full" },
                { "build_property.SourceExpander_Embedder_Include", "/home/**" },
            };

            var embeddedNamespaces = ImmutableArray<string>.Empty;
            var embeddedFiles = ImmutableArray.Create(
                 new SourceFileInfo
                 (
                     "TestProject>Program.cs",
                     ["Program"],
                     ImmutableArray.Create("using System;"),
                     ImmutableArray<string>.Empty,
                     """class Program{static void Main(){Console.WriteLine(1);}[System.Diagnostics.Conditional("TEST")]static void T()=>Console.WriteLine(2);}"""
                 ));
            const string embeddedSourceCode = "[{\"CodeBody\":\"class Program{static void Main(){Console.WriteLine(1);}[System.Diagnostics.Conditional(\\\"TEST\\\")]static void T()=>Console.WriteLine(2);}\",\"Dependencies\":[],\"FileName\":\"TestProject>Program.cs\",\"TypeNames\":[\"Program\"],\"Usings\":[\"using System;\"]}]";

            var test = new Test
            {
                AnalyzerConfigOptionsProvider = analyzerConfigOptionsProvider,
                TestState =
                {
                    Sources = {
                        (
                            "/other/Dummy.cs",
                            @"class Dummy{}"
                        ),
                        (
                            "/other/Dummy2.cs",
                            @"class Dummy2{}"
                        ),
                        (
                            "/home/source/Program.cs",
                            """
using System;
using System.Diagnostics;

class Program
{
    static void Main()
    {
        Console.WriteLine(1);
    }

    [System.Diagnostics.Conditional("TEST")]
    static void T() => Console.WriteLine(2);
}
"""
                        ),
                    },
                    GeneratedSources =
                    {
                        (typeof(EmbedderGenerator), "EmbeddedSourceCode.Metadata.cs",$"""
                        using System.Reflection;
                        [assembly: AssemblyMetadataAttribute("SourceExpander.EmbedderVersion","{EmbedderVersion}")]
                        [assembly: AssemblyMetadataAttribute("SourceExpander.EmbeddedLanguageVersion","{EmbeddedLanguageVersion}")]
                        [assembly: AssemblyMetadataAttribute("SourceExpander.EmbeddedNamespaces","{string.Join(",", embeddedNamespaces)}")]
                        [assembly: AssemblyMetadataAttribute("SourceExpander.EmbeddedSourceCode",{embeddedSourceCode.ToLiteral()})]
                        
                        """
                        ),
                    }
                }
            };
            await test.RunAsync();
            Newtonsoft.Json.JsonConvert.DeserializeObject<SourceFileInfo[]>(embeddedSourceCode)
                .Should()
                .BeEquivalentTo(embeddedFiles);
            System.Text.Json.JsonSerializer.Deserialize<SourceFileInfo[]>(embeddedSourceCode)
                .Should()
                .BeEquivalentTo(embeddedFiles);
        }

        public static TheoryData<string[]> Exclude_Data => new()
        {
            new []{ "/other/**" },
            new []{ "/other/**/*.cs" },
            new []{ "/other/Dummy.cs", "/other/Dummy2.cs" },
        };

        [Theory]
        [MemberData(nameof(Exclude_Data))]
        public async Task Exclude(string[] data)
        {
            var additionalText = new InMemorySourceText(
                "/foo/bar/SourceExpander.Embedder.Config.json", $$$"""
{
    "$schema": "https://raw.githubusercontent.com/kzrnm/SourceExpander/master/schema/embedder.schema.json",
    "embedding-type": "Raw",
    "exclude": "{{{data}}}",
    "minify-level": "full"
}
""");

            var embeddedNamespaces = ImmutableArray<string>.Empty;
            var embeddedFiles = ImmutableArray.Create(
                 new SourceFileInfo
                 (
                     "TestProject>Program.cs",
                     ["Program"],
                     ImmutableArray.Create("using System;"),
                     ImmutableArray<string>.Empty,
                     @"class Program{static void Main(){Console.WriteLine(1);}[System.Diagnostics.Conditional(""TEST"")]static void T()=>Console.WriteLine(2);}"
                 ));
            const string embeddedSourceCode = "[{\"CodeBody\":\"class Program{static void Main(){Console.WriteLine(1);}[System.Diagnostics.Conditional(\\\"TEST\\\")]static void T()=>Console.WriteLine(2);}\",\"Dependencies\":[],\"FileName\":\"TestProject>Program.cs\",\"TypeNames\":[\"Program\"],\"Usings\":[\"using System;\"]}]";

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
                            "/other/Dummy.cs",
                            @"class Dummy{}"
                        ),
                        (
                            "/other/Dummy2.cs",
                            @"class Dummy2{}"
                        ),
                        (
                            "/home/source/Program.cs",
                            @"
using System;
using System.Diagnostics;

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
                        (typeof(EmbedderGenerator), "EmbeddedSourceCode.Metadata.cs",$"""
                        using System.Reflection;
                        [assembly: AssemblyMetadataAttribute("SourceExpander.EmbedderVersion","{EmbedderVersion}")]
                        [assembly: AssemblyMetadataAttribute("SourceExpander.EmbeddedLanguageVersion","{EmbeddedLanguageVersion}")]
                        [assembly: AssemblyMetadataAttribute("SourceExpander.EmbeddedNamespaces","{string.Join(",", embeddedNamespaces)}")]
                        [assembly: AssemblyMetadataAttribute("SourceExpander.EmbeddedSourceCode",{embeddedSourceCode.ToLiteral()})]
                        
                        """
                        ),
                    }
                }
            };
            await test.RunAsync();
            Newtonsoft.Json.JsonConvert.DeserializeObject<SourceFileInfo[]>(embeddedSourceCode)
                .Should()
                .BeEquivalentTo(embeddedFiles);
            System.Text.Json.JsonSerializer.Deserialize<SourceFileInfo[]>(embeddedSourceCode)
                .Should()
                .BeEquivalentTo(embeddedFiles);
        }

        [Fact]
        public async Task ExcludeProperty()
        {
            var analyzerConfigOptionsProvider = new DummyAnalyzerConfigOptionsProvider
            {
                { "build_property.SourceExpander_Embedder_EmbeddingType", "raw" },
                { "build_property.SourceExpander_Embedder_MinifyLevel", "full" },
                { "build_property.SourceExpander_Embedder_Exclude", "/other/**" }
            };

            var embeddedNamespaces = ImmutableArray<string>.Empty;
            var embeddedFiles = ImmutableArray.Create(
                 new SourceFileInfo
                 (
                     "TestProject>Program.cs",
                     ["Program"],
                     ImmutableArray.Create("using System;"),
                     ImmutableArray<string>.Empty,
                     """class Program{static void Main(){Console.WriteLine(1);}[System.Diagnostics.Conditional("TEST")]static void T()=>Console.WriteLine(2);}"""
                 ));
            const string embeddedSourceCode = "[{\"CodeBody\":\"class Program{static void Main(){Console.WriteLine(1);}[System.Diagnostics.Conditional(\\\"TEST\\\")]static void T()=>Console.WriteLine(2);}\",\"Dependencies\":[],\"FileName\":\"TestProject>Program.cs\",\"TypeNames\":[\"Program\"],\"Usings\":[\"using System;\"]}]";

            var test = new Test
            {
                AnalyzerConfigOptionsProvider = analyzerConfigOptionsProvider,
                TestState =
                {
                    Sources = {
                        (
                            "/other/Dummy.cs",
                            @"class Dummy{}"
                        ),
                        (
                            "/other/Dummy2.cs",
                            @"class Dummy2{}"
                        ),
                        (
                            "/home/source/Program.cs",
                            """
using System;
using System.Diagnostics;

class Program
{
    static void Main()
    {
        Console.WriteLine(1);
    }

    [System.Diagnostics.Conditional("TEST")]
    static void T() => Console.WriteLine(2);
}
"""
                        ),
                    },
                    GeneratedSources =
                    {
                        (typeof(EmbedderGenerator), "EmbeddedSourceCode.Metadata.cs",$"""
                        using System.Reflection;
                        [assembly: AssemblyMetadataAttribute("SourceExpander.EmbedderVersion","{EmbedderVersion}")]
                        [assembly: AssemblyMetadataAttribute("SourceExpander.EmbeddedLanguageVersion","{EmbeddedLanguageVersion}")]
                        [assembly: AssemblyMetadataAttribute("SourceExpander.EmbeddedNamespaces","{string.Join(",", embeddedNamespaces)}")]
                        [assembly: AssemblyMetadataAttribute("SourceExpander.EmbeddedSourceCode",{embeddedSourceCode.ToLiteral()})]
                        
                        """
                        ),
                    }
                }
            };
            await test.RunAsync();
            Newtonsoft.Json.JsonConvert.DeserializeObject<SourceFileInfo[]>(embeddedSourceCode)
                .Should()
                .BeEquivalentTo(embeddedFiles);
            System.Text.Json.JsonSerializer.Deserialize<SourceFileInfo[]>(embeddedSourceCode)
                .Should()
                .BeEquivalentTo(embeddedFiles);
        }
    }
}
