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
            var opts = new CSharpParseOptions(kind: SourceCodeKind.Regular, documentationMode: DocumentationMode.Parse);
            var driver = CSharpGeneratorDriver.Create(
                new[] { generator },
                additionalTexts: new[] { additionalText },
                parseOptions: opts);
            driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);
            diagnostics.Should().BeEmpty();
            outputCompilation.GetDiagnostics().Should().BeEmpty();
            outputCompilation.SyntaxTrees.Should().HaveCount(1);

            var metadata = outputCompilation.Assembly.GetAttributes()
                .Where(x => x.AttributeClass?.Name == nameof(System.Reflection.AssemblyMetadataAttribute))
                .ToDictionary(x => (string)x.ConstructorArguments[0].Value, x => (string)x.ConstructorArguments[1].Value);
            metadata.Should().NotContainKey("SourceExpander.EmbeddedSourceCode");
            metadata.Should().NotContainKey("SourceExpander.EmbeddedSourceCode.GZipBase32768");

            outputCompilation.SyntaxTrees.Should().HaveCount(1);
            diagnostics.Should().BeEmpty();

            outputCompilation.SyntaxTrees
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
            var opts = new CSharpParseOptions(kind: SourceCodeKind.Regular, documentationMode: DocumentationMode.Parse);
            var driver = CSharpGeneratorDriver.Create(
                new[] { generator },
                additionalTexts: new[] { additionalText },
                parseOptions: opts);
            driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);
            diagnostics.Should().BeEmpty();
            outputCompilation.GetDiagnostics().Should().BeEmpty();
            outputCompilation.SyntaxTrees.Should().HaveCount(2);

            var metadata = outputCompilation.Assembly.GetAttributes()
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

            outputCompilation.SyntaxTrees.Should().HaveCount(2);
            diagnostics.Should().BeEmpty();

            outputCompilation.SyntaxTrees
                .Should()
                .ContainSingle(tree => tree.GetRoot(default).ToString().Contains("[assembly: AssemblyMetadataAttribute(\"SourceExpander"))
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
            var opts = new CSharpParseOptions(kind: SourceCodeKind.Regular, documentationMode: DocumentationMode.Parse);
            var driver = CSharpGeneratorDriver.Create(
                new[] { generator },
                additionalTexts: new[] { additionalText },
                parseOptions: opts);
            driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);
            diagnostics.Should().BeEmpty();
            outputCompilation.GetDiagnostics().Should().BeEmpty();
            outputCompilation.SyntaxTrees.Should().HaveCount(2);

            var metadata = outputCompilation.Assembly.GetAttributes()
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

            outputCompilation.SyntaxTrees.Should().HaveCount(2);
            diagnostics.Should().BeEmpty();

            outputCompilation.SyntaxTrees
                .Should()
                .ContainSingle(tree => tree.GetRoot(default).ToString().Contains("[assembly: AssemblyMetadataAttribute(\"SourceExpander"))
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
            var opts = new CSharpParseOptions(kind: SourceCodeKind.Regular, documentationMode: DocumentationMode.Parse);
            var driver = CSharpGeneratorDriver.Create(
                new[] { generator },
                additionalTexts: new[] {
                    additionalText,
                    new InMemoryAdditionalText("/foo/bar/SourceExpander.Notmatch.json", "notmatch"),
                },
                parseOptions: opts);
            driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out var diagnostics);
            diagnostics.Should().ContainSingle().Which.Id.Should().Be("EMBED0003");
        }
    }
}
