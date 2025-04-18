﻿using System.Collections.Immutable;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace SourceExpander.Generate.Config
{
    public class ExpandInLibraryTest : EmbedderGeneratorTestBase
    {
        [Fact]
        public async Task NotExpandJson()
        {
            InMemorySourceText additionalText = new("/foo/bar/SourceExpander.Embedder.Config.json", """
{
    "$schema": "https://raw.githubusercontent.com/kzrnm/SourceExpander/master/schema/embedder.schema.json",
    "expand-in-library": false,
    "embedding-type": "Raw",
    "exclude-attributes": [
        "System.Diagnostics.DebuggerDisplayAttribute"
    ],
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
                     @"class Program{static void Main(){Console.WriteLine(1);}}"
                 ));
            const string embeddedSourceCode = "[{\"CodeBody\":\"class Program{static void Main(){Console.WriteLine(1);}}\",\"Dependencies\":[],\"FileName\":\"TestProject>Program.cs\",\"TypeNames\":[\"Program\"],\"Usings\":[\"using System;\"]}]";

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

    [SourceExpander.NotEmbeddingSource]
    static void T() => Console.WriteLine(2);
}
"
                        ),
                    },
                    GeneratedSources =
                    {
                        (typeof(EmbedderGenerator), "EmbeddedSourceCode.Metadata.cs",$"""
                        // <auto-generated/>
                        #pragma warning disable
                        [assembly: global::System.Reflection.AssemblyMetadataAttribute("SourceExpander.EmbedderVersion","{EmbedderVersion}")]
                        [assembly: global::System.Reflection.AssemblyMetadataAttribute("SourceExpander.EmbeddedLanguageVersion","{EmbeddedLanguageVersion}")]
                        [assembly: global::System.Reflection.AssemblyMetadataAttribute("SourceExpander.EmbeddedNamespaces","{string.Join(",", embeddedNamespaces)}")]
                        [assembly: global::System.Reflection.AssemblyMetadataAttribute("SourceExpander.EmbeddedSourceCode",{embeddedSourceCode.ToLiteral()})]
                        
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
        public async Task NotExpandProperty()
        {
            var analyzerConfigOptionsProvider = new DummyAnalyzerConfigOptionsProvider
            {
                { "build_property.SourceExpander_Embedder_ExpandInLibrary", "False" },
                { "build_property.SourceExpander_Embedder_EmbeddingType", "raw" },
                { "build_property.SourceExpander_Embedder_MinifyLevel", "full" },
                { "build_property.SourceExpander_Embedder_ExcludeAttributes", "System.Diagnostics.DebuggerDisplayAttribute;System.Diagnostics.DebuggerDisplayAttribute" },
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
                            "/home/source/Program.cs",
                            """
using System;
using System.Diagnostics;

[DebuggerDisplay("Name")]
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
                        // <auto-generated/>
                        #pragma warning disable
                        [assembly: global::System.Reflection.AssemblyMetadataAttribute("SourceExpander.EmbedderVersion","{EmbedderVersion}")]
                        [assembly: global::System.Reflection.AssemblyMetadataAttribute("SourceExpander.EmbeddedLanguageVersion","{EmbeddedLanguageVersion}")]
                        [assembly: global::System.Reflection.AssemblyMetadataAttribute("SourceExpander.EmbeddedNamespaces","{string.Join(",", embeddedNamespaces)}")]
                        [assembly: global::System.Reflection.AssemblyMetadataAttribute("SourceExpander.EmbeddedSourceCode",{embeddedSourceCode.ToLiteral()})]
                        
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
        public async Task ExpandJson()
        {
            InMemorySourceText additionalText = new("/foo/bar/SourceExpander.Embedder.Config.json", """
{
    "$schema": "https://raw.githubusercontent.com/kzrnm/SourceExpander/master/schema/embedder.schema.json",
    "expand-in-library": true,
    "embedding-type": "Raw",
    "exclude-attributes": [
        "System.Diagnostics.DebuggerDisplayAttribute"
    ],
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
                     @"class Program{static void Main(){Console.WriteLine(1);}}"
                 ));
            const string embeddedSourceCode = "[{\"CodeBody\":\"class Program{static void Main(){Console.WriteLine(1);}}\",\"Dependencies\":[],\"FileName\":\"TestProject>Program.cs\",\"TypeNames\":[\"Program\"],\"Usings\":[\"using System;\"]}]";

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

    [SourceExpander.NotEmbeddingSource]
    static void T() => Console.WriteLine(2);
}
"
                        ),
                    },
                    GeneratedSources =
                    {
                        (typeof(EmbedderGenerator), "EmbeddedSourceCode.Metadata.cs",$"""
                        // <auto-generated/>
                        #pragma warning disable
                        [assembly: global::System.Reflection.AssemblyMetadataAttribute("SourceExpander.EmbedderVersion","{EmbedderVersion}")]
                        [assembly: global::System.Reflection.AssemblyMetadataAttribute("SourceExpander.EmbeddedLanguageVersion","{EmbeddedLanguageVersion}")]
                        [assembly: global::System.Reflection.AssemblyMetadataAttribute("SourceExpander.EmbeddedNamespaces","{string.Join(",", embeddedNamespaces)}")]
                        [assembly: global::System.Reflection.AssemblyMetadataAttribute("SourceExpander.EmbeddedSourceCode",{embeddedSourceCode.ToLiteral()})]
                        
                        """
                        ),
                        (typeof(EmbedderGenerator), "ExpandInLibrary.cs","""
                        // <auto-generated/>
                        #pragma warning disable
                        namespace SourceExpander.Embedded.Expand{
                        using System;
                        class Program{static void Main(){Console.WriteLine(1);}}

                        }

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
        public async Task ExpandProperty()
        {
            var analyzerConfigOptionsProvider = new DummyAnalyzerConfigOptionsProvider
            {
                { "build_property.SourceExpander_Embedder_ExpandInLibrary", "true" },
                { "build_property.SourceExpander_Embedder_EmbeddingType", "raw" },
                { "build_property.SourceExpander_Embedder_MinifyLevel", "full" },
                { "build_property.SourceExpander_Embedder_ExcludeAttributes", "System.Diagnostics.DebuggerDisplayAttribute;System.Diagnostics.DebuggerDisplayAttribute" },
            };

            var embeddedNamespaces = ImmutableArray<string>.Empty;
            var embeddedFiles = ImmutableArray.Create(
                 new SourceFileInfo
                 (
                     "TestProject>Program.cs",
                     ["Program"],
                     ImmutableArray.Create("using System;"),
                     ImmutableArray<string>.Empty,
                     """class Program{static void Main(){Console.WriteLine(1);}}"""
                 ));
            const string embeddedSourceCode = "[{\"CodeBody\":\"class Program{static void Main(){Console.WriteLine(1);}}\",\"Dependencies\":[],\"FileName\":\"TestProject>Program.cs\",\"TypeNames\":[\"Program\"],\"Usings\":[\"using System;\"]}]";

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
using System.Diagnostics;

[DebuggerDisplay("Name")]
class Program
{
    static void Main()
    {
        Console.WriteLine(1);
    }

    [SourceExpander.NotEmbeddingSource]
    static void T() => Console.WriteLine(2);
}
"""
                        ),
                    },
                    GeneratedSources =
                    {
                        (typeof(EmbedderGenerator), "EmbeddedSourceCode.Metadata.cs",$"""
                        // <auto-generated/>
                        #pragma warning disable
                        [assembly: global::System.Reflection.AssemblyMetadataAttribute("SourceExpander.EmbedderVersion","{EmbedderVersion}")]
                        [assembly: global::System.Reflection.AssemblyMetadataAttribute("SourceExpander.EmbeddedLanguageVersion","{EmbeddedLanguageVersion}")]
                        [assembly: global::System.Reflection.AssemblyMetadataAttribute("SourceExpander.EmbeddedNamespaces","{string.Join(",", embeddedNamespaces)}")]
                        [assembly: global::System.Reflection.AssemblyMetadataAttribute("SourceExpander.EmbeddedSourceCode",{embeddedSourceCode.ToLiteral()})]
                        
                        """
                        ),
                        (typeof(EmbedderGenerator), "ExpandInLibrary.cs","""
                        // <auto-generated/>
                        #pragma warning disable
                        namespace SourceExpander.Embedded.Expand{
                        using System;
                        class Program{static void Main(){Console.WriteLine(1);}}

                        }

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
