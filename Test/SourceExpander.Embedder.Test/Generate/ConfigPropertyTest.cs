using System.Collections.Immutable;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace SourceExpander.Generate
{
    public class ConfigPropertyTest : EmbedderGeneratorTestBase
    {
        [Fact]
        public async Task NotEnabled()
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

        [Fact]
        public async Task ExcludeAttributes()
        {
            var analyzerConfigOptionsProvider = new DummyAnalyzerConfigOptionsProvider
            {
                { "build_property.SourceExpander_Embedder_EmbeddingType", "raw" },
                { "build_property.SourceExpander_Embedder_MinifyLevel", "full" },
                { "build_property.SourceExpander_Embedder_ExcludeAttributes", "System.Diagnostics.DebuggerDisplayAttribute;System.Diagnostics.DebuggerDisplayAttribute" },
            };

            var embeddedNamespaces = ImmutableArray<string>.Empty;
            var embeddedFiles = ImmutableArray.Create(
                 new SourceFileInfo
                 (
                     "TestProject>Program.cs",
                     new string[] { "Program" },
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
        public async Task EmbeddingRaw()
        {
            var analyzerConfigOptionsProvider = new DummyAnalyzerConfigOptionsProvider
            {
                { "build_property.SourceExpander_Embedder_EmbeddingType", "raw" },
            };

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
            const string embeddedSourceCode = "[{\"CodeBody\":\"[DebuggerDisplay(\\\"Name\\\")] class Program { static void Main() { Console.WriteLine(1); }  [System.Diagnostics.Conditional(\\\"TEST\\\")] static void T() => Console.WriteLine(2); }\",\"Dependencies\":[],\"FileName\":\"TestProject>Program.cs\",\"TypeNames\":[\"Program\"],\"Usings\":[\"using System;\",\"using System.Diagnostics;\"]}]";

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
        public async Task EmbeddingRawNoMinify()
        {
            var analyzerConfigOptionsProvider = new DummyAnalyzerConfigOptionsProvider
            {
                { "build_property.SourceExpander_Embedder_EmbeddingType", "raw" },
                { "build_property.SourceExpander_Embedder_MinifyLevel", "off" },
            };

            var embeddedNamespaces = ImmutableArray<string>.Empty;
            var embeddedFiles = ImmutableArray.Create(
                 new SourceFileInfo
                 (
                     "TestProject>Program.cs",
                     new string[] { "Program" },
                     ImmutableArray.Create("using System;", "using System.Diagnostics;"),
                     ImmutableArray<string>.Empty,
                     @"[DebuggerDisplay(""Name"")]
class Program
{
    static void Main()
    {
        Console.WriteLine(1);
    }

    [System.Diagnostics.Conditional(""TEST"")]
    static void T() => Console.WriteLine(2);
}".Replace("\r\n", "\n")
                 ));
            const string embeddedSourceCode = "[{\"CodeBody\":\"[DebuggerDisplay(\\\"Name\\\")]\\nclass Program\\n{\\n    static void Main()\\n    {\\n        Console.WriteLine(1);\\n    }\\n\\n    [System.Diagnostics.Conditional(\\\"TEST\\\")]\\n    static void T() => Console.WriteLine(2);\\n}\",\"Dependencies\":[],\"FileName\":\"TestProject>Program.cs\",\"TypeNames\":[\"Program\"],\"Usings\":[\"using System;\",\"using System.Diagnostics;\"]}]";

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
        public async Task EmbeddingRawFullMinify()
        {
            var analyzerConfigOptionsProvider = new DummyAnalyzerConfigOptionsProvider
            {
                { "build_property.SourceExpander_Embedder_EmbeddingType", "raw" },
                { "build_property.SourceExpander_Embedder_MinifyLevel", "full" },
            };

            var embeddedNamespaces = ImmutableArray<string>.Empty;
            var embeddedFiles = ImmutableArray.Create(
                 new SourceFileInfo
                 (
                     "TestProject>Program.cs",
                     new string[] { "Program" },
                     ImmutableArray.Create("using System;", "using System.Diagnostics;"),
                     ImmutableArray<string>.Empty,
                     @"[DebuggerDisplay(""Name"")]class Program{static void Main(){Console.WriteLine(1);}[System.Diagnostics.Conditional(""TEST"")]static void T()=>Console.WriteLine(2);}"
                 ));
            const string embeddedSourceCode = "[{\"CodeBody\":\"[DebuggerDisplay(\\\"Name\\\")]class Program{static void Main(){Console.WriteLine(1);}[System.Diagnostics.Conditional(\\\"TEST\\\")]static void T()=>Console.WriteLine(2);}\",\"Dependencies\":[],\"FileName\":\"TestProject>Program.cs\",\"TypeNames\":[\"Program\"],\"Usings\":[\"using System;\",\"using System.Diagnostics;\"]}]";

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
        public async Task EmbeddingGzipBase32768()
        {
            var analyzerConfigOptionsProvider = new DummyAnalyzerConfigOptionsProvider
            {
                { "build_property.SourceExpander_Embedder_EmbeddingType", "gzip-base32768" },
                { "build_property.SourceExpander_Embedder_MinifyLevel", "full" },
            };

            var embeddedNamespaces = ImmutableArray<string>.Empty;
            var embeddedFiles = ImmutableArray.Create(
                 new SourceFileInfo
                 (
                     "TestProject>Program.cs",
                     new string[] { "Program" },
                     ImmutableArray.Create("using System;", "using System.Diagnostics;"),
                     ImmutableArray<string>.Empty,
                     @"[DebuggerDisplay(""Name"")]class Program{static void Main(){Console.WriteLine(1);}[System.Diagnostics.Conditional(""TEST"")]static void T()=>Console.WriteLine(2);}"
                 ));
            string embeddedSourceCode = SourceFileInfoUtil.ToGZipBase32768("[{\"CodeBody\":\"[DebuggerDisplay(\\\"Name\\\")]class Program{static void Main(){Console.WriteLine(1);}[System.Diagnostics.Conditional(\\\"TEST\\\")]static void T()=>Console.WriteLine(2);}\",\"Dependencies\":[],\"FileName\":\"TestProject>Program.cs\",\"TypeNames\":[\"Program\"],\"Usings\":[\"using System;\",\"using System.Diagnostics;\"]}]");

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
        public async Task ConditionalNone()
        {
            var analyzerConfigOptionsProvider = new DummyAnalyzerConfigOptionsProvider
            {
                { "build_property.SourceExpander_Embedder_EmbeddingType", "raw" },
                { "build_property.SourceExpander_Embedder_MinifyLevel", "full" },
            };

            var embeddedNamespaces = ImmutableArray<string>.Empty;
            var embeddedFiles = ImmutableArray.Create(
                 new SourceFileInfo
                 (
                     "TestProject>Program.cs",
                     new string[] { "Program" },
                     ImmutableArray.Create("using System;", "using System.Diagnostics;"),
                     ImmutableArray<string>.Empty,
                     @"class Program{static void Main(){Debug.Assert(true);Console.WriteLine(1);}}"
                 ));
            const string embeddedSourceCode = "[{\"CodeBody\":\"class Program{static void Main(){Debug.Assert(true);Console.WriteLine(1);}}\",\"Dependencies\":[],\"FileName\":\"TestProject>Program.cs\",\"TypeNames\":[\"Program\"],\"Usings\":[\"using System;\",\"using System.Diagnostics;\"]}]";

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

class Program
{
    static void Main()
    {
        Debug.Assert(true);
        Console.WriteLine(1);
    }
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
        public async Task Conditional()
        {
            var analyzerConfigOptionsProvider = new DummyAnalyzerConfigOptionsProvider
            {
                { "build_property.SourceExpander_Embedder_EmbeddingType", "Raw" },
                { "build_property.SourceExpander_Embedder_MinifyLevel", "full" },
                { "build_property.SourceExpander_Embedder_RemoveConditional", "DEBUG;DEBUG2" },
            };

            var embeddedNamespaces = ImmutableArray<string>.Empty;
            var embeddedFiles = ImmutableArray.Create(
                 new SourceFileInfo
                 (
                     "TestProject>Program.cs",
                     new string[] { "Program" },
                     ImmutableArray.Create("using System;", "using System.Diagnostics;"),
                     ImmutableArray<string>.Empty,
                     @"class Program{static void Main(){T();Console.WriteLine(1);}[System.Diagnostics.Conditional(""TEST"")]static void T()=>Console.WriteLine(2);[Conditional(""DEBUG2"")]static void T4()=>Console.WriteLine(4);[System.Diagnostics.Conditional(""DEBUG2"")][Conditional(""Test"")]static void T8()=>Console.WriteLine(8);}"
                 ));
            const string embeddedSourceCode = "[{\"CodeBody\":\"class Program{static void Main(){T();Console.WriteLine(1);}[System.Diagnostics.Conditional(\\\"TEST\\\")]static void T()=>Console.WriteLine(2);[Conditional(\\\"DEBUG2\\\")]static void T4()=>Console.WriteLine(4);[System.Diagnostics.Conditional(\\\"DEBUG2\\\")][Conditional(\\\"Test\\\")]static void T8()=>Console.WriteLine(8);}\",\"Dependencies\":[],\"FileName\":\"TestProject>Program.cs\",\"TypeNames\":[\"Program\"],\"Usings\":[\"using System;\",\"using System.Diagnostics;\"]}]";

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

class Program
{
    static void Main()
    {
        Debug.Assert(true);
        T();
        T4();
        T8();
        Console.WriteLine(1);
    }

    [System.Diagnostics.Conditional(""TEST"")]
    static void T() => Console.WriteLine(2);
    [Conditional(""DEBUG2"")]
    static void T4() => Console.WriteLine(4);
    [System.Diagnostics.Conditional(""DEBUG2"")]
    [Conditional(""Test"")]
    static void T8() => Console.WriteLine(8);
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
        public async Task EmbeddingFileNameTypeWithoutCommonPrefix()
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
