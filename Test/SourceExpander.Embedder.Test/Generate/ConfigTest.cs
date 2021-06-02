using System.Collections.Immutable;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace SourceExpander.Generate
{
    public class ConfigTest : EmbedderGeneratorTestBase
    {
        public static TheoryData ParseErrorJsons = new TheoryData<InMemorySourceText, object[]>
        {
            {
                new InMemorySourceText(
                "/foo/directory/SourceExpander.Embedder.Config.json", @"
{
    ""$schema"": ""https://raw.githubusercontent.com/naminodarie/SourceExpander/master/schema/embedder.schema.json"",
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
    ""$schema"": ""https://raw.githubusercontent.com/naminodarie/SourceExpander/master/schema/embedder.schema.json"",
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
                    ExpectedDiagnostics =
                    {
                        DiagnosticResult.CompilerError("EMBED0003")
                            .WithSpan(additionalText.Path, 1, 1, 1, 1)
                            .WithArguments(diagnosticsArg),
                    }
                }
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task NotEnabled()
        {
            var additionalText = new InMemorySourceText(
                "/foo/bar/SourceExpander.Embedder.Config.json", @"
{
    ""$schema"": ""https://raw.githubusercontent.com/naminodarie/SourceExpander/master/schema/embedder.schema.json"",
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
        public async Task ExcludeAttributes()
        {
            var additionalText = new InMemorySourceText(
                "/foo/bar/SourceExpander.Embedder.Config.json", @"
{
    ""$schema"": ""https://raw.githubusercontent.com/naminodarie/SourceExpander/master/schema/embedder.schema.json"",
    ""embedding-type"": ""Raw"",
    ""exclude-attributes"": [
        ""System.Diagnostics.DebuggerDisplayAttribute""
    ],
    ""minify-level"": ""full""
}
");

            var embeddedFiles = ImmutableArray.Create(
                 new SourceFileInfo
                 (
                     "TestProject>Program.cs",
                     new string[] { "Program" },
                     ImmutableArray.Create("using System;"),
                     ImmutableArray.Create<string>(),
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
                        (typeof(EmbedderGenerator), "EmbeddedSourceCode.Metadata.cs", @$"using System.Reflection;
[assembly: AssemblyMetadataAttribute(""SourceExpander.EmbedderVersion"",""{EmbedderVersion}"")]
[assembly: AssemblyMetadataAttribute(""SourceExpander.EmbeddedLanguageVersion"",""{EmbeddedLanguageVersion}"")]
[assembly: AssemblyMetadataAttribute(""SourceExpander.EmbeddedSourceCode"",{embeddedSourceCode.ToLiteral()})]
"),
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
            var additionalText = new InMemorySourceText(
                "/foo/bar/SourceExpander.Embedder.Config.json", @"
{
    ""$schema"": ""https://raw.githubusercontent.com/naminodarie/SourceExpander/master/schema/embedder.schema.json"",
    ""embedding-type"": ""Raw""
}
");

            var embeddedFiles = ImmutableArray.Create(
                 new SourceFileInfo
                 (
                     "TestProject>Program.cs",
                     new string[] { "Program" },
                     ImmutableArray.Create("using System;", "using System.Diagnostics;"),
                     ImmutableArray.Create<string>(),
                     @"[DebuggerDisplay(""Name"")] class Program { static void Main() { Console.WriteLine(1); }  [System.Diagnostics.Conditional(""TEST"")] static void T() => Console.WriteLine(2); }"
                 ));
            const string embeddedSourceCode = "[{\"CodeBody\":\"[DebuggerDisplay(\\\"Name\\\")] class Program { static void Main() { Console.WriteLine(1); }  [System.Diagnostics.Conditional(\\\"TEST\\\")] static void T() => Console.WriteLine(2); }\",\"Dependencies\":[],\"FileName\":\"TestProject>Program.cs\",\"TypeNames\":[\"Program\"],\"Usings\":[\"using System;\",\"using System.Diagnostics;\"]}]";

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
                        (typeof(EmbedderGenerator), "EmbeddedSourceCode.Metadata.cs", @$"using System.Reflection;
[assembly: AssemblyMetadataAttribute(""SourceExpander.EmbedderVersion"",""{EmbedderVersion}"")]
[assembly: AssemblyMetadataAttribute(""SourceExpander.EmbeddedLanguageVersion"",""{EmbeddedLanguageVersion}"")]
[assembly: AssemblyMetadataAttribute(""SourceExpander.EmbeddedSourceCode"",{embeddedSourceCode.ToLiteral()})]
"),
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
            var additionalText = new InMemorySourceText(
                "/foo/bar/SourceExpander.Embedder.Config.json", @"
{
    ""$schema"": ""https://raw.githubusercontent.com/naminodarie/SourceExpander/master/schema/embedder.schema.json"",
    ""embedding-type"": ""Raw"",
    ""minify-level"": ""off""
}
");

            var embeddedFiles = ImmutableArray.Create(
                 new SourceFileInfo
                 (
                     "TestProject>Program.cs",
                     new string[] { "Program" },
                     ImmutableArray.Create("using System;", "using System.Diagnostics;"),
                     ImmutableArray.Create<string>(),
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
                        (typeof(EmbedderGenerator), "EmbeddedSourceCode.Metadata.cs", @$"using System.Reflection;
[assembly: AssemblyMetadataAttribute(""SourceExpander.EmbedderVersion"",""{EmbedderVersion}"")]
[assembly: AssemblyMetadataAttribute(""SourceExpander.EmbeddedLanguageVersion"",""{EmbeddedLanguageVersion}"")]
[assembly: AssemblyMetadataAttribute(""SourceExpander.EmbeddedSourceCode"",{embeddedSourceCode.ToLiteral()})]
"),
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
            var additionalText = new InMemorySourceText(
                "/foo/bar/SourceExpander.Embedder.Config.json", @"
{
    ""$schema"": ""https://raw.githubusercontent.com/naminodarie/SourceExpander/master/schema/embedder.schema.json"",
    ""embedding-type"": ""Raw"",
    ""minify-level"": ""full""
}
");

            var embeddedFiles = ImmutableArray.Create(
                 new SourceFileInfo
                 (
                     "TestProject>Program.cs",
                     new string[] { "Program" },
                     ImmutableArray.Create("using System;", "using System.Diagnostics;"),
                     ImmutableArray.Create<string>(),
                     @"[DebuggerDisplay(""Name"")]class Program{static void Main(){Console.WriteLine(1);}[System.Diagnostics.Conditional(""TEST"")]static void T()=>Console.WriteLine(2);}"
                 ));
            const string embeddedSourceCode = "[{\"CodeBody\":\"[DebuggerDisplay(\\\"Name\\\")]class Program{static void Main(){Console.WriteLine(1);}[System.Diagnostics.Conditional(\\\"TEST\\\")]static void T()=>Console.WriteLine(2);}\",\"Dependencies\":[],\"FileName\":\"TestProject>Program.cs\",\"TypeNames\":[\"Program\"],\"Usings\":[\"using System;\",\"using System.Diagnostics;\"]}]";

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
                        (typeof(EmbedderGenerator), "EmbeddedSourceCode.Metadata.cs", @$"using System.Reflection;
[assembly: AssemblyMetadataAttribute(""SourceExpander.EmbedderVersion"",""{EmbedderVersion}"")]
[assembly: AssemblyMetadataAttribute(""SourceExpander.EmbeddedLanguageVersion"",""{EmbeddedLanguageVersion}"")]
[assembly: AssemblyMetadataAttribute(""SourceExpander.EmbeddedSourceCode"",{embeddedSourceCode.ToLiteral()})]
"),
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
            var additionalText = new InMemorySourceText(
                "/foo/bar/SourceExpander.Embedder.Config.json", @"
{
    ""$schema"": ""https://raw.githubusercontent.com/naminodarie/SourceExpander/master/schema/embedder.schema.json"",
    ""embedding-type"": ""gzip-base32768"",
    ""minify-level"": ""full""
}
");

            var embeddedFiles = ImmutableArray.Create(
                 new SourceFileInfo
                 (
                     "TestProject>Program.cs",
                     new string[] { "Program" },
                     ImmutableArray.Create("using System;", "using System.Diagnostics;"),
                     ImmutableArray.Create<string>(),
                     @"[DebuggerDisplay(""Name"")]class Program{static void Main(){Console.WriteLine(1);}[System.Diagnostics.Conditional(""TEST"")]static void T()=>Console.WriteLine(2);}"
                 ));
            string embeddedSourceCode = SourceFileInfoUtil.ToGZipBase32768("[{\"CodeBody\":\"[DebuggerDisplay(\\\"Name\\\")]class Program{static void Main(){Console.WriteLine(1);}[System.Diagnostics.Conditional(\\\"TEST\\\")]static void T()=>Console.WriteLine(2);}\",\"Dependencies\":[],\"FileName\":\"TestProject>Program.cs\",\"TypeNames\":[\"Program\"],\"Usings\":[\"using System;\",\"using System.Diagnostics;\"]}]");

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
                        (typeof(EmbedderGenerator), "EmbeddedSourceCode.Metadata.cs", @$"using System.Reflection;
[assembly: AssemblyMetadataAttribute(""SourceExpander.EmbedderVersion"",""{EmbedderVersion}"")]
[assembly: AssemblyMetadataAttribute(""SourceExpander.EmbeddedLanguageVersion"",""{EmbeddedLanguageVersion}"")]
[assembly: AssemblyMetadataAttribute(""SourceExpander.EmbeddedSourceCode.GZipBase32768"",{embeddedSourceCode.ToLiteral()})]
"),
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
            var additionalText = new InMemorySourceText(
                "/foo/bar/SourceExpander.Embedder.Config.json", @"
{
    ""$schema"": ""https://raw.githubusercontent.com/naminodarie/SourceExpander/master/schema/embedder.schema.json"",
    ""embedding-type"": ""Raw"",
    ""minify-level"": ""full""
}
");

            var embeddedFiles = ImmutableArray.Create(
                 new SourceFileInfo
                 (
                     "TestProject>Program.cs",
                     new string[] { "Program" },
                     ImmutableArray.Create("using System;", "using System.Diagnostics;"),
                     ImmutableArray.Create<string>(),
                     @"class Program{static void Main(){Debug.Assert(true);Console.WriteLine(1);}}"
                 ));
            const string embeddedSourceCode = "[{\"CodeBody\":\"class Program{static void Main(){Debug.Assert(true);Console.WriteLine(1);}}\",\"Dependencies\":[],\"FileName\":\"TestProject>Program.cs\",\"TypeNames\":[\"Program\"],\"Usings\":[\"using System;\",\"using System.Diagnostics;\"]}]";

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
                        (typeof(EmbedderGenerator), "EmbeddedSourceCode.Metadata.cs", @$"using System.Reflection;
[assembly: AssemblyMetadataAttribute(""SourceExpander.EmbedderVersion"",""{EmbedderVersion}"")]
[assembly: AssemblyMetadataAttribute(""SourceExpander.EmbeddedLanguageVersion"",""{EmbeddedLanguageVersion}"")]
[assembly: AssemblyMetadataAttribute(""SourceExpander.EmbeddedSourceCode"",{embeddedSourceCode.ToLiteral()})]
"),
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
            var additionalText = new InMemorySourceText(
                "/foo/bar/SourceExpander.Embedder.Config.json", @"
{
    ""$schema"": ""https://raw.githubusercontent.com/naminodarie/SourceExpander/master/schema/embedder.schema.json"",
    ""embedding-type"": ""Raw"",
    ""remove-conditional"": [
        ""DEBUG"", ""DEBUG2""
    ],
    ""minify-level"": ""full""
}
");

            var embeddedFiles = ImmutableArray.Create(
                 new SourceFileInfo
                 (
                     "TestProject>Program.cs",
                     new string[] { "Program" },
                     ImmutableArray.Create("using System;", "using System.Diagnostics;"),
                     ImmutableArray.Create<string>(),
                     @"class Program{static void Main(){T();Console.WriteLine(1);}[System.Diagnostics.Conditional(""TEST"")]static void T()=>Console.WriteLine(2);[Conditional(""DEBUG2"")]static void T4()=>Console.WriteLine(4);[System.Diagnostics.Conditional(""DEBUG2"")][Conditional(""Test"")]static void T8()=>Console.WriteLine(8);}"
                 ));
            const string embeddedSourceCode = "[{\"CodeBody\":\"class Program{static void Main(){T();Console.WriteLine(1);}[System.Diagnostics.Conditional(\\\"TEST\\\")]static void T()=>Console.WriteLine(2);[Conditional(\\\"DEBUG2\\\")]static void T4()=>Console.WriteLine(4);[System.Diagnostics.Conditional(\\\"DEBUG2\\\")][Conditional(\\\"Test\\\")]static void T8()=>Console.WriteLine(8);}\",\"Dependencies\":[],\"FileName\":\"TestProject>Program.cs\",\"TypeNames\":[\"Program\"],\"Usings\":[\"using System;\",\"using System.Diagnostics;\"]}]";

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
                        (typeof(EmbedderGenerator), "EmbeddedSourceCode.Metadata.cs", @$"using System.Reflection;
[assembly: AssemblyMetadataAttribute(""SourceExpander.EmbedderVersion"",""{EmbedderVersion}"")]
[assembly: AssemblyMetadataAttribute(""SourceExpander.EmbeddedLanguageVersion"",""{EmbeddedLanguageVersion}"")]
[assembly: AssemblyMetadataAttribute(""SourceExpander.EmbeddedSourceCode"",{embeddedSourceCode.ToLiteral()})]
"),
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
        public async Task EmbeddingSourceClassNone()
        {
            var additionalText = new InMemorySourceText(
                "/foo/bar/SourceExpander.Embedder.Config.json", @"
{
    ""$schema"": ""https://raw.githubusercontent.com/naminodarie/SourceExpander/master/schema/embedder.schema.json"",
    ""embedding-type"": ""Raw"",
    ""embedding-source-class"": {
        ""enabled"": false,
        ""if-directive"": ""SOURCEEXPANDER""
    },
    ""minify-level"": ""full""
}
");

            var embeddedFiles = ImmutableArray.Create(
                 new SourceFileInfo
                 (
                     "TestProject>Program.cs",
                     new string[] { "Program" },
                     ImmutableArray.Create("using System;", "using System.Diagnostics;"),
                     ImmutableArray.Create<string>(),
                     @"class Program{static void Main(){Debug.Assert(true);Console.WriteLine(1);}}"
                 ));
            const string embeddedSourceCode = "[{\"CodeBody\":\"class Program{static void Main(){Debug.Assert(true);Console.WriteLine(1);}}\",\"Dependencies\":[],\"FileName\":\"TestProject>Program.cs\",\"TypeNames\":[\"Program\"],\"Usings\":[\"using System;\",\"using System.Diagnostics;\"]}]";

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
                        (typeof(EmbedderGenerator), "EmbeddedSourceCode.Metadata.cs", @$"using System.Reflection;
[assembly: AssemblyMetadataAttribute(""SourceExpander.EmbedderVersion"",""{EmbedderVersion}"")]
[assembly: AssemblyMetadataAttribute(""SourceExpander.EmbeddedLanguageVersion"",""{EmbeddedLanguageVersion}"")]
[assembly: AssemblyMetadataAttribute(""SourceExpander.EmbeddedSourceCode"",{embeddedSourceCode.ToLiteral()})]
"),
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
        public async Task EmbeddingSourceClass()
        {
            var additionalText = new InMemorySourceText(
                "/foo/bar/SourceExpander.Embedder.Config.json", @"
{
    ""$schema"": ""https://raw.githubusercontent.com/naminodarie/SourceExpander/master/schema/embedder.schema.json"",
    ""embedding-type"": ""Raw"",
    ""embedding-source-class"": {
        ""enabled"": true,
        ""class-name"": ""Container""
    },
    ""minify-level"": ""full""
}
");

            var embeddedFiles = ImmutableArray.Create(
                 new SourceFileInfo
                 (
                     "TestProject>Program.cs",
                     new string[] { "Program" },
                     ImmutableArray.Create("using System;", "using System.Diagnostics;"),
                     ImmutableArray.Create<string>(),
                     @"class Program{static void Main(){Debug.Assert(true);Console.WriteLine(1);}}"
                 ));
            const string embeddedSourceCode = "[{\"CodeBody\":\"class Program{static void Main(){Debug.Assert(true);Console.WriteLine(1);}}\",\"Dependencies\":[],\"FileName\":\"TestProject>Program.cs\",\"TypeNames\":[\"Program\"],\"Usings\":[\"using System;\",\"using System.Diagnostics;\"]}]";

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
                        (typeof(EmbedderGenerator), "EmbeddingSourceClass.cs", @"namespace SourceExpander.Embedded{
using System;
using System.Collections.Generic;
public class Container{
public class SourceFileInfo{
  public string FileName{get;set;}
  public string[] TypeNames{get;set;}
  public string[] Usings{get;set;}
  public string[] Dependencies{get;set;}
  public string CodeBody{get;set;}
}
  public static readonly IReadOnlyList<SourceFileInfo> Files = new SourceFileInfo[]{
    new SourceFileInfo{
      FileName = ""TestProject>Program.cs"",
      CodeBody = ""class Program{static void Main(){Debug.Assert(true);Console.WriteLine(1);}}"",
      TypeNames = new string[]{
        ""Program"",
      },
      Usings = new string[]{
        ""using System;"",
        ""using System.Diagnostics;"",
      },
      Dependencies = new string[]{
      },
    },
  };
}
}
"
                        ),
                        (typeof(EmbedderGenerator), "EmbeddedSourceCode.Metadata.cs", @$"using System.Reflection;
[assembly: AssemblyMetadataAttribute(""SourceExpander.EmbedderVersion"",""{EmbedderVersion}"")]
[assembly: AssemblyMetadataAttribute(""SourceExpander.EmbeddedLanguageVersion"",""{EmbeddedLanguageVersion}"")]
[assembly: AssemblyMetadataAttribute(""SourceExpander.EmbeddedSourceCode"",{embeddedSourceCode.ToLiteral()})]
"),
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
        public async Task EmbeddingSourceClassDefault()
        {
            var additionalText = new InMemorySourceText(
                "/foo/bar/SourceExpander.Embedder.Config.json", @"
{
    ""$schema"": ""https://raw.githubusercontent.com/naminodarie/SourceExpander/master/schema/embedder.schema.json"",
    ""embedding-type"": ""Raw"",
    ""embedding-source-class"": {
        ""enabled"": true
    },
    ""minify-level"": ""full""
}
");

            var embeddedFiles = ImmutableArray.Create(
                 new SourceFileInfo
                 (
                     "TestProject>Program.cs",
                     new string[] { "Program" },
                     ImmutableArray.Create("using System;", "using System.Diagnostics;"),
                     ImmutableArray.Create<string>(),
                     @"class Program{static void Main(){Debug.Assert(true);Console.WriteLine(1);}}"
                 ));
            const string embeddedSourceCode = "[{\"CodeBody\":\"class Program{static void Main(){Debug.Assert(true);Console.WriteLine(1);}}\",\"Dependencies\":[],\"FileName\":\"TestProject>Program.cs\",\"TypeNames\":[\"Program\"],\"Usings\":[\"using System;\",\"using System.Diagnostics;\"]}]";

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
                        (typeof(EmbedderGenerator), "EmbeddingSourceClass.cs", @"namespace SourceExpander.Embedded{
using System;
using System.Collections.Generic;
public class SourceFileInfoContainer{
public class SourceFileInfo{
  public string FileName{get;set;}
  public string[] TypeNames{get;set;}
  public string[] Usings{get;set;}
  public string[] Dependencies{get;set;}
  public string CodeBody{get;set;}
}
  public static readonly IReadOnlyList<SourceFileInfo> Files = new SourceFileInfo[]{
    new SourceFileInfo{
      FileName = ""TestProject>Program.cs"",
      CodeBody = ""class Program{static void Main(){Debug.Assert(true);Console.WriteLine(1);}}"",
      TypeNames = new string[]{
        ""Program"",
      },
      Usings = new string[]{
        ""using System;"",
        ""using System.Diagnostics;"",
      },
      Dependencies = new string[]{
      },
    },
  };
}
}
"
                        ),
                        (typeof(EmbedderGenerator), "EmbeddedSourceCode.Metadata.cs", @$"using System.Reflection;
[assembly: AssemblyMetadataAttribute(""SourceExpander.EmbedderVersion"",""{EmbedderVersion}"")]
[assembly: AssemblyMetadataAttribute(""SourceExpander.EmbeddedLanguageVersion"",""{EmbeddedLanguageVersion}"")]
[assembly: AssemblyMetadataAttribute(""SourceExpander.EmbeddedSourceCode"",{embeddedSourceCode.ToLiteral()})]
"),
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
        public async Task EmbeddingSourceClassAlways()
        {
            var additionalText = new InMemorySourceText(
                "/foo/bar/SourceExpander.Embedder.Config.json", @"
{
    ""$schema"": ""https://raw.githubusercontent.com/naminodarie/SourceExpander/master/schema/embedder.schema.json"",
    ""embedding-type"": ""Raw"",
    ""embedding-source-class"": {
        ""enabled"": true,
        ""class-name"": """"
    },
    ""minify-level"": ""full""
}
");

            var embeddedFiles = ImmutableArray.Create(
                 new SourceFileInfo
                 (
                     "TestProject>Program.cs",
                     new string[] { "Program" },
                     ImmutableArray.Create("using System;", "using System.Diagnostics;"),
                     ImmutableArray.Create<string>(),
                     @"class Program{static void Main(){Debug.Assert(true);Console.WriteLine(1);}}"
                 ));
            const string embeddedSourceCode = "[{\"CodeBody\":\"class Program{static void Main(){Debug.Assert(true);Console.WriteLine(1);}}\",\"Dependencies\":[],\"FileName\":\"TestProject>Program.cs\",\"TypeNames\":[\"Program\"],\"Usings\":[\"using System;\",\"using System.Diagnostics;\"]}]";

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
                        (typeof(EmbedderGenerator), "EmbeddingSourceClass.cs", @"namespace SourceExpander.Embedded{
using System;
using System.Collections.Generic;
public class SourceFileInfoContainer{
public class SourceFileInfo{
  public string FileName{get;set;}
  public string[] TypeNames{get;set;}
  public string[] Usings{get;set;}
  public string[] Dependencies{get;set;}
  public string CodeBody{get;set;}
}
  public static readonly IReadOnlyList<SourceFileInfo> Files = new SourceFileInfo[]{
    new SourceFileInfo{
      FileName = ""TestProject>Program.cs"",
      CodeBody = ""class Program{static void Main(){Debug.Assert(true);Console.WriteLine(1);}}"",
      TypeNames = new string[]{
        ""Program"",
      },
      Usings = new string[]{
        ""using System;"",
        ""using System.Diagnostics;"",
      },
      Dependencies = new string[]{
      },
    },
  };
}
}
"
                        ),
                        (typeof(EmbedderGenerator), "EmbeddedSourceCode.Metadata.cs", @$"using System.Reflection;
[assembly: AssemblyMetadataAttribute(""SourceExpander.EmbedderVersion"",""{EmbedderVersion}"")]
[assembly: AssemblyMetadataAttribute(""SourceExpander.EmbeddedLanguageVersion"",""{EmbeddedLanguageVersion}"")]
[assembly: AssemblyMetadataAttribute(""SourceExpander.EmbeddedSourceCode"",{embeddedSourceCode.ToLiteral()})]
"),
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

        public static TheoryData ObsoleteConfigProperty_Data = new TheoryData<InMemorySourceText, (string, string)[]>
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
        };

        [Theory]
        [MemberData(nameof(ObsoleteConfigProperty_Data))]
        public async Task ObsoleteConfigProperty(InMemorySourceText additionalText, (string Obsolete, string Instead)[] diagnosticsArgs)
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

    }
}
