using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace SourceExpander.Embedder.Generate.Test
{
    public class ConfigTest : EmbeddingGeneratorTestBase
    {
        static readonly CSharpParseOptions parseOptions = new CSharpParseOptions(kind: SourceCodeKind.Regular, documentationMode: DocumentationMode.Parse);
        static readonly CSharpCompilationOptions compilationOptions
            = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                    .WithSpecificDiagnosticOptions(new Dictionary<string, ReportDiagnostic> {
                        {
                            "CS8019",ReportDiagnostic.Suppress
                        },
                    });

        static SyntaxTree CreateSyntaxTree(string code, string path = null)
            => CSharpSyntaxTree.ParseText(code, path: path ?? "/home/source/Program.cs");

        [Fact]
        public void NotEnabled()
        {
            var additionalText = new InMemoryAdditionalText(
                "/foo/bar/SourceExpander.Embedder.Config.json", @"
{
    ""$schema"": ""https://raw.githubusercontent.com/naminodarie/SourceExpander/master/schema/embedder.schema.json"",
    ""enabled"": false,
    ""exclude-attributes"": [
        ""System.Diagnostics.DebuggerDisplayAttribute""
    ]
}
");

            var compilation = CreateCompilation(new[] {
                CreateSyntaxTree(@"
using System;

class Program
{
    static void Main()
    {
        Console.WriteLine(1);
    }
}
")
            }, compilationOptions, new[] { expanderCoreReference });
            compilation.GetDiagnostics().Should().BeEmpty();

            var generator = new EmbedderGenerator();
            var gen = RunGenerator(compilation, generator, new[] { additionalText }, parseOptions);
            gen.Diagnostics.Should().BeEmpty();
            gen.OutputCompilation.GetDiagnostics().Should().BeEmpty();
            gen.OutputCompilation.SyntaxTrees.Should().HaveCount(1);

            var metadata = gen.OutputCompilation.Assembly.GetAttributes()
                .Where(x => x.AttributeClass?.Name == nameof(System.Reflection.AssemblyMetadataAttribute))
                .ToDictionary(x => (string)x.ConstructorArguments[0].Value, x => (string)x.ConstructorArguments[1].Value);
            metadata.Should().NotContainKey("SourceExpander.EmbeddedSourceCode");
            metadata.Should().NotContainKey("SourceExpander.EmbeddedSourceCode.GZipBase32768");

            gen.OutputCompilation.SyntaxTrees.Should().HaveCount(1);
            gen.Diagnostics.Should().BeEmpty();

            gen.OutputCompilation.SyntaxTrees
                .Should()
                .NotContain(tree => tree.GetRoot(default).ToString().Contains("[assembly: AssemblyMetadataAttribute(\"SourceExpander"));
        }

        [Fact]
        public void ExcludeAttributes()
        {
            var additionalText = new InMemoryAdditionalText(
                "/foo/bar/SourceExpander.Embedder.Config.json", @"
{
    ""$schema"": ""https://raw.githubusercontent.com/naminodarie/SourceExpander/master/schema/embedder.schema.json"",
    ""exclude-attributes"": [
        ""System.Diagnostics.DebuggerDisplayAttribute""
    ],
    ""enable-minify"": true
}
");

            var compilation = CreateCompilation(new[] {
                CreateSyntaxTree(@"
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
")
            }, compilationOptions, new[] { expanderCoreReference });
            compilation.GetDiagnostics().Should().BeEmpty();

            var embeddedFiles = ImmutableArray.Create(
                 new SourceFileInfo
                 (
                     "TestAssembly>Program.cs",
                     new string[] { "Program" },
                     ImmutableArray.Create("using System;"),
                     ImmutableArray.Create<string>(),
                     @"class Program{static void Main(){Console.WriteLine(1);}[System.Diagnostics.Conditional(""TEST"")]static void T()=>Console.WriteLine(2);}"
                 ));

            var generator = new EmbedderGenerator();
            var gen = RunGenerator(compilation, generator, new[] { additionalText }, parseOptions);
            gen.Diagnostics.Should().BeEmpty();
            gen.OutputCompilation.GetDiagnostics().Should().BeEmpty();
            gen.OutputCompilation.SyntaxTrees.Should().HaveCount(2);

            var metadata = gen.OutputCompilation.Assembly.GetAttributes()
                .Where(x => x.AttributeClass?.Name == nameof(System.Reflection.AssemblyMetadataAttribute))
                .ToDictionary(x => (string)x.ConstructorArguments[0].Value, x => (string)x.ConstructorArguments[1].Value);
            metadata.Should().NotContainKey("SourceExpander.EmbeddedSourceCode");
            metadata.Should().ContainKey("SourceExpander.EmbeddedSourceCode.GZipBase32768");

            var embedded = metadata["SourceExpander.EmbeddedSourceCode.GZipBase32768"];
            Newtonsoft.Json.JsonConvert.DeserializeObject<SourceFileInfo[]>(
                SourceFileInfoUtil.FromGZipBase32768(embedded))
                .Should()
                .BeEquivalentTo(embeddedFiles);
            System.Text.Json.JsonSerializer.Deserialize<SourceFileInfo[]>(
                SourceFileInfoUtil.FromGZipBase32768(embedded))
                .Should()
                .BeEquivalentTo(embeddedFiles);

            gen.OutputCompilation.SyntaxTrees.Should().HaveCount(2);
            gen.Diagnostics.Should().BeEmpty();


            gen.AddedSyntaxTrees
                .Should()
                .ContainSingle()
                .Which
                .ToString()
                .Should()
                .ContainAll(
                "[assembly: AssemblyMetadataAttribute(\"SourceExpander.EmbeddedSourceCode.GZipBase32768\",",
                "[assembly: AssemblyMetadataAttribute(\"SourceExpander.EmbedderVersion\","
                )
                .And
                .NotContain("[assembly: AssemblyMetadataAttribute(\"SourceExpander.EmbeddedAllowUnsafe\",");
        }

        [Fact]
        public void EmbeddingRaw()
        {
            var additionalText = new InMemoryAdditionalText(
                "/foo/bar/SourceExpander.Embedder.Config.json", @"
{
    ""$schema"": ""https://raw.githubusercontent.com/naminodarie/SourceExpander/master/schema/embedder.schema.json"",
    ""embedding-type"": ""Raw"",
    ""enable-minify"": true
}
");

            var compilation = CreateCompilation(new[] {
                CreateSyntaxTree(@"
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
")
            }, compilationOptions, new[] { expanderCoreReference });
            compilation.GetDiagnostics().Should().BeEmpty();

            var embeddedFiles = ImmutableArray.Create(
                 new SourceFileInfo
                 (
                     "TestAssembly>Program.cs",
                     new string[] { "Program" },
                     ImmutableArray.Create("using System;", "using System.Diagnostics;"),
                     ImmutableArray.Create<string>(),
                     @"[DebuggerDisplay(""Name"")]class Program{static void Main(){Console.WriteLine(1);}[System.Diagnostics.Conditional(""TEST"")]static void T()=>Console.WriteLine(2);}"
                 ));

            var generator = new EmbedderGenerator();
            var gen = RunGenerator(compilation, generator, new[] { additionalText }, parseOptions);
            gen.Diagnostics.Should().BeEmpty();
            gen.OutputCompilation.GetDiagnostics().Should().BeEmpty();
            gen.OutputCompilation.SyntaxTrees.Should().HaveCount(2);

            var metadata = gen.OutputCompilation.Assembly.GetAttributes()
                .Where(x => x.AttributeClass?.Name == nameof(System.Reflection.AssemblyMetadataAttribute))
                .ToDictionary(x => (string)x.ConstructorArguments[0].Value, x => (string)x.ConstructorArguments[1].Value);
            metadata.Should().NotContainKey("SourceExpander.EmbeddedSourceCode.GZipBase32768");
            metadata.Should().ContainKey("SourceExpander.EmbeddedSourceCode");

            var embedded = metadata["SourceExpander.EmbeddedSourceCode"];
            Newtonsoft.Json.JsonConvert.DeserializeObject<SourceFileInfo[]>(embedded)
                .Should()
                .BeEquivalentTo(embeddedFiles);
            System.Text.Json.JsonSerializer.Deserialize<SourceFileInfo[]>(embedded)
                .Should()
                .BeEquivalentTo(embeddedFiles);

            gen.OutputCompilation.SyntaxTrees.Should().HaveCount(2);
            gen.Diagnostics.Should().BeEmpty();
            gen.AddedSyntaxTrees
                .Should()
                .ContainSingle()
                .Which
                .ToString()
                .Should()
                .ContainAll(
                "[assembly: AssemblyMetadataAttribute(\"SourceExpander.EmbeddedSourceCode\",",
                "[assembly: AssemblyMetadataAttribute(\"SourceExpander.EmbedderVersion\","
                )
                .And
                .NotContain("[assembly: AssemblyMetadataAttribute(\"SourceExpander.EmbeddedAllowUnsafe\",");
        }

        [Fact]
        public void NoConditional()
        {
            var additionalText = new InMemoryAdditionalText(
                "/foo/bar/SourceExpander.Embedder.Config.json", @"
{
    ""$schema"": ""https://raw.githubusercontent.com/naminodarie/SourceExpander/master/schema/embedder.schema.json"",
    ""enable-minify"": true
}
");

            var compilation = CreateCompilation(new[] {
                CreateSyntaxTree(@"
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
")
            }, compilationOptions, new[] { expanderCoreReference });
            compilation.GetDiagnostics().Should().BeEmpty();
            var embeddedFiles = ImmutableArray.Create(
                 new SourceFileInfo
                 (
                     "TestAssembly>Program.cs",
                     new string[] { "Program" },
                     ImmutableArray.Create("using System;", "using System.Diagnostics;"),
                     ImmutableArray.Create<string>(),
                     @"class Program{static void Main(){Debug.Assert(true);Console.WriteLine(1);}}"
                 ));

            var generator = new EmbedderGenerator();
            var gen = RunGenerator(compilation, generator, new[] { additionalText }, parseOptions);
            gen.Diagnostics.Should().BeEmpty();
            gen.OutputCompilation.GetDiagnostics().Should().BeEmpty();
            gen.OutputCompilation.SyntaxTrees.Should().HaveCount(2);

            var metadata = gen.OutputCompilation.Assembly.GetAttributes()
                .Where(x => x.AttributeClass?.Name == nameof(System.Reflection.AssemblyMetadataAttribute))
                .ToDictionary(x => (string)x.ConstructorArguments[0].Value, x => (string)x.ConstructorArguments[1].Value);
            metadata.Should().ContainKey("SourceExpander.EmbeddedSourceCode.GZipBase32768");
            metadata.Should().NotContainKey("SourceExpander.EmbeddedSourceCode");

            var embedded = metadata["SourceExpander.EmbeddedSourceCode.GZipBase32768"];
            Newtonsoft.Json.JsonConvert.DeserializeObject<SourceFileInfo[]>(
                SourceFileInfoUtil.FromGZipBase32768(embedded))
                .Should()
                .BeEquivalentTo(embeddedFiles);
            System.Text.Json.JsonSerializer.Deserialize<SourceFileInfo[]>(
                SourceFileInfoUtil.FromGZipBase32768(embedded))
                .Should()
                .BeEquivalentTo(embeddedFiles);

            gen.OutputCompilation.SyntaxTrees.Should().HaveCount(2);
            gen.Diagnostics.Should().BeEmpty();


            gen.AddedSyntaxTrees
                .Should()
                .ContainSingle()
                .Which
                .ToString()
                .Should()
                .ContainAll(
                "[assembly: AssemblyMetadataAttribute(\"SourceExpander.EmbeddedSourceCode.GZipBase32768\",",
                "[assembly: AssemblyMetadataAttribute(\"SourceExpander.EmbedderVersion\","
                )
                .And
                .NotContain("[assembly: AssemblyMetadataAttribute(\"SourceExpander.EmbeddedAllowUnsafe\",");
        }

        [Fact]
        public void Conditional()
        {
            var additionalText = new InMemoryAdditionalText(
                "/foo/bar/SourceExpander.Embedder.Config.json", @"
{
    ""$schema"": ""https://raw.githubusercontent.com/naminodarie/SourceExpander/master/schema/embedder.schema.json"",
    ""remove-conditional"": [
        ""DEBUG"", ""DEBUG2""
    ],
    ""enable-minify"": true
}
");

            var compilation = CreateCompilation(new[] {
                CreateSyntaxTree(@"
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
")
            }, compilationOptions, new[] { expanderCoreReference });
            compilation.GetDiagnostics().Should().BeEmpty();

            var embeddedFiles = ImmutableArray.Create(
                 new SourceFileInfo
                 (
                     "TestAssembly>Program.cs",
                     new string[] { "Program" },
                     ImmutableArray.Create("using System;", "using System.Diagnostics;"),
                     ImmutableArray.Create<string>(),
                     @"class Program{static void Main(){T();Console.WriteLine(1);}[System.Diagnostics.Conditional(""TEST"")]static void T()=>Console.WriteLine(2);[Conditional(""DEBUG2"")]static void T4()=>Console.WriteLine(4);[System.Diagnostics.Conditional(""DEBUG2"")][Conditional(""Test"")]static void T8()=>Console.WriteLine(8);}"
                 ));

            var generator = new EmbedderGenerator();
            var gen = RunGenerator(compilation, generator, new[] { additionalText }, parseOptions);
            gen.Diagnostics.Should().BeEmpty();
            gen.OutputCompilation.GetDiagnostics().Should().BeEmpty();
            gen.OutputCompilation.SyntaxTrees.Should().HaveCount(2);

            var metadata = gen.OutputCompilation.Assembly.GetAttributes()
                .Where(x => x.AttributeClass?.Name == nameof(System.Reflection.AssemblyMetadataAttribute))
                .ToDictionary(x => (string)x.ConstructorArguments[0].Value, x => (string)x.ConstructorArguments[1].Value);
            metadata.Should().ContainKey("SourceExpander.EmbeddedSourceCode.GZipBase32768");
            metadata.Should().NotContainKey("SourceExpander.EmbeddedSourceCode");

            var embedded = metadata["SourceExpander.EmbeddedSourceCode.GZipBase32768"];
            Newtonsoft.Json.JsonConvert.DeserializeObject<SourceFileInfo[]>(
                SourceFileInfoUtil.FromGZipBase32768(embedded))
                .Should()
                .BeEquivalentTo(embeddedFiles);
            System.Text.Json.JsonSerializer.Deserialize<SourceFileInfo[]>(
                SourceFileInfoUtil.FromGZipBase32768(embedded))
                .Should()
                .BeEquivalentTo(embeddedFiles);

            gen.OutputCompilation.SyntaxTrees.Should().HaveCount(2);
            gen.Diagnostics.Should().BeEmpty();

            gen.AddedSyntaxTrees
                .Should()
                .ContainSingle()
                .Which
                .ToString()
                .Should()
                .ContainAll(
                "[assembly: AssemblyMetadataAttribute(\"SourceExpander.EmbeddedSourceCode.GZipBase32768\",",
                "[assembly: AssemblyMetadataAttribute(\"SourceExpander.EmbedderVersion\","
                )
                .And
                .NotContain("[assembly: AssemblyMetadataAttribute(\"SourceExpander.EmbeddedAllowUnsafe\",");
        }

        public static TheoryData ParseErrorJsons = new TheoryData<InMemoryAdditionalText>
        {
            {
                new InMemoryAdditionalText(
                "/foo/bar/SourceExpander.Embedder.Config.json", @"
{
    ""$schema"": ""https://raw.githubusercontent.com/naminodarie/SourceExpander/master/schema/embedder.schema.json"",
    ""exclude-attributes"": 1
}
")
            },
            {
                new InMemoryAdditionalText(
                "/foo/bar/sourceExpander.embedder.config.json", @"
{
    ""$schema"": ""https://raw.githubusercontent.com/naminodarie/SourceExpander/master/schema/embedder.schema.json"",
    ""exclude-attributes"": 1
}
")
            },
        };

        [Theory]
        [MemberData(nameof(ParseErrorJsons))]
        public void ParseErrorTest(InMemoryAdditionalText additionalText)
        {
            var compilation = CreateCompilation(new[] {
                CreateSyntaxTree(@"
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
")
            }, compilationOptions, new[] { expanderCoreReference });
            compilation.GetDiagnostics().Should().BeEmpty();

            var generator = new EmbedderGenerator();
            RunGenerator(compilation, generator,
                    additionalTexts: new[] {
                                    additionalText,
                                    new InMemoryAdditionalText("/foo/bar/SourceExpander.Notmatch.json", "notmatch"),
                    },
                    parseOptions: parseOptions)
                .Diagnostics.Should().ContainSingle().Which.Id.Should().Be("EMBED0003");
        }
    }
}
