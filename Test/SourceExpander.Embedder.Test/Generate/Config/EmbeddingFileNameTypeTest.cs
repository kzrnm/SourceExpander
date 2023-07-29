﻿using System.Collections.Immutable;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace SourceExpander.Generate.Config
{
    public class EmbeddingFileNameTypeTest : EmbedderGeneratorTestBase
    {
        [Fact]
        public async Task EmbeddingFileNameTypeWithoutCommonPrefix()
        {
            var additionalText = new InMemorySourceText(
                "/foo/bar/SourceExpander.Embedder.Config.json", @"
{
    ""$schema"": ""https://raw.githubusercontent.com/kzrnm/SourceExpander/master/schema/embedder.schema.json"",
    ""embedding-type"": ""Raw"",
    ""embedding-filename-type"": ""withoutCommonPrefix""
}
");

            var embeddedNamespaces = ImmutableArray<string>.Empty;
            var embeddedFiles = ImmutableArray.Create(
                 new SourceFileInfo
                 (
                     "TestProject>rogram.cs",
                     new string[] { "Program" },
                     ImmutableArray.Create("using System;", "using System.Diagnostics;"),
                     ImmutableArray<string>.Empty,
                     @"[DebuggerDisplay(""Name"")] class Program { static void Main() { Console.WriteLine(1); }  [System.Diagnostics.Conditional(""TEST"")] static void T() => Console.WriteLine(2); }"
                 ),
                 new SourceFileInfo
                 (
                     "TestProject>.cs",
                     new string[] { "P" },
                     ImmutableArray.Create("using System;"),
                     ImmutableArray<string>.Empty,
                     @"class P { static void T() => Console.WriteLine(2); }"
                 ));
            const string embeddedSourceCode = "[{\"CodeBody\":\"class P { static void T() => Console.WriteLine(2); }\",\"Dependencies\":[],\"FileName\":\"TestProject>.cs\",\"TypeNames\":[\"P\"],\"Usings\":[\"using System;\"]},{\"CodeBody\":\"[DebuggerDisplay(\\\"Name\\\")] class Program { static void Main() { Console.WriteLine(1); }  [System.Diagnostics.Conditional(\\\"TEST\\\")] static void T() => Console.WriteLine(2); }\",\"Dependencies\":[],\"FileName\":\"TestProject>rogram.cs\",\"TypeNames\":[\"Program\"],\"Usings\":[\"using System;\",\"using System.Diagnostics;\"]}]";

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
                        (
                            "/home/source/P.cs",
                            @"
using System;
class P
{
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
        public async Task EmbeddingFileNameTypeWithoutCommonPrefixProperty()
        {
            var analyzerConfigOptionsProvider = new DummyAnalyzerConfigOptionsProvider
            {
                { "build_property.SourceExpander_Embedder_EmbeddingType", "raw" },
                { "build_property.SourceExpander_Embedder_EmbeddingFileNameType", "withoutCommonPrefix" },
            };

            var embeddedNamespaces = ImmutableArray<string>.Empty;
            var embeddedFiles = ImmutableArray.Create(
                 new SourceFileInfo
                 (
                     "TestProject>rogram.cs",
                     new string[] { "Program" },
                     ImmutableArray.Create("using System;", "using System.Diagnostics;"),
                     ImmutableArray<string>.Empty,
                     @"[DebuggerDisplay(""Name"")] class Program { static void Main() { Console.WriteLine(1); }  [System.Diagnostics.Conditional(""TEST"")] static void T() => Console.WriteLine(2); }"
                 ),
                 new SourceFileInfo
                 (
                     "TestProject>.cs",
                     new string[] { "P" },
                     ImmutableArray.Create("using System;"),
                     ImmutableArray<string>.Empty,
                     @"class P { static void T() => Console.WriteLine(2); }"
                 ));
            const string embeddedSourceCode = "[{\"CodeBody\":\"class P { static void T() => Console.WriteLine(2); }\",\"Dependencies\":[],\"FileName\":\"TestProject>.cs\",\"TypeNames\":[\"P\"],\"Usings\":[\"using System;\"]},{\"CodeBody\":\"[DebuggerDisplay(\\\"Name\\\")] class Program { static void Main() { Console.WriteLine(1); }  [System.Diagnostics.Conditional(\\\"TEST\\\")] static void T() => Console.WriteLine(2); }\",\"Dependencies\":[],\"FileName\":\"TestProject>rogram.cs\",\"TypeNames\":[\"Program\"],\"Usings\":[\"using System;\",\"using System.Diagnostics;\"]}]";

            var test = new Test
            {
                AnalyzerConfigOptionsProvider = analyzerConfigOptionsProvider,
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
                        (
                            "/home/source/P.cs",
                            @"
using System;
class P
{
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
        public async Task EmbeddingFileNameTypeFullPath()
        {
            var additionalText = new InMemorySourceText(
                "/foo/bar/SourceExpander.Embedder.Config.json", @"
{
    ""$schema"": ""https://raw.githubusercontent.com/kzrnm/SourceExpander/master/schema/embedder.schema.json"",
    ""embedding-type"": ""Raw"",
    ""embedding-filename-type"": ""fullpath""
}
");

            var embeddedNamespaces = ImmutableArray<string>.Empty;
            var embeddedFiles = ImmutableArray.Create(
                 new SourceFileInfo
                 (
                     "/home/source/Program.cs",
                     new string[] { "Program" },
                     ImmutableArray.Create("using System;", "using System.Diagnostics;"),
                     ImmutableArray<string>.Empty,
                     @"[DebuggerDisplay(""Name"")] class Program { static void Main() { Console.WriteLine(1); }  [System.Diagnostics.Conditional(""TEST"")] static void T() => Console.WriteLine(2); }"
                 ),
                 new SourceFileInfo
                 (
                     "/home/source/P.cs",
                     new string[] { "P" },
                     ImmutableArray.Create("using System;"),
                     ImmutableArray<string>.Empty,
                     @"class P { static void T() => Console.WriteLine(2); }"
                 ));
            const string embeddedSourceCode = "[{\"CodeBody\":\"class P { static void T() => Console.WriteLine(2); }\",\"Dependencies\":[],\"FileName\":\"/home/source/P.cs\",\"TypeNames\":[\"P\"],\"Usings\":[\"using System;\"]},{\"CodeBody\":\"[DebuggerDisplay(\\\"Name\\\")] class Program { static void Main() { Console.WriteLine(1); }  [System.Diagnostics.Conditional(\\\"TEST\\\")] static void T() => Console.WriteLine(2); }\",\"Dependencies\":[],\"FileName\":\"/home/source/Program.cs\",\"TypeNames\":[\"Program\"],\"Usings\":[\"using System;\",\"using System.Diagnostics;\"]}]";

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
                        (
                            "/home/source/P.cs",
                            @"
using System;
class P
{
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
        public async Task EmbeddingFileNameTypeFullPathProperty()
        {
            var analyzerConfigOptionsProvider = new DummyAnalyzerConfigOptionsProvider
            {
                { "build_property.SourceExpander_Embedder_EmbeddingType", "raw" },
                { "build_property.SourceExpander_Embedder_EmbeddingFileNameType", "fullpath" },
            };

            var embeddedNamespaces = ImmutableArray<string>.Empty;
            var embeddedFiles = ImmutableArray.Create(
                 new SourceFileInfo
                 (
                     "/home/source/Program.cs",
                     new string[] { "Program" },
                     ImmutableArray.Create("using System;", "using System.Diagnostics;"),
                     ImmutableArray<string>.Empty,
                     @"[DebuggerDisplay(""Name"")] class Program { static void Main() { Console.WriteLine(1); }  [System.Diagnostics.Conditional(""TEST"")] static void T() => Console.WriteLine(2); }"
                 ),
                 new SourceFileInfo
                 (
                     "/home/source/P.cs",
                     new string[] { "P" },
                     ImmutableArray.Create("using System;"),
                     ImmutableArray<string>.Empty,
                     @"class P { static void T() => Console.WriteLine(2); }"
                 ));
            const string embeddedSourceCode = "[{\"CodeBody\":\"class P { static void T() => Console.WriteLine(2); }\",\"Dependencies\":[],\"FileName\":\"/home/source/P.cs\",\"TypeNames\":[\"P\"],\"Usings\":[\"using System;\"]},{\"CodeBody\":\"[DebuggerDisplay(\\\"Name\\\")] class Program { static void Main() { Console.WriteLine(1); }  [System.Diagnostics.Conditional(\\\"TEST\\\")] static void T() => Console.WriteLine(2); }\",\"Dependencies\":[],\"FileName\":\"/home/source/Program.cs\",\"TypeNames\":[\"Program\"],\"Usings\":[\"using System;\",\"using System.Diagnostics;\"]}]";

            var test = new Test
            {
                AnalyzerConfigOptionsProvider = analyzerConfigOptionsProvider,
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
                        (
                            "/home/source/P.cs",
                            @"
using System;
class P
{
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
                        [assembly: AssemblyMetadataAttribute("SourceExpander.EmbeddedSourceCode",{embeddedSourceCode.ToLiteral()})]
                        
                        """),
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
