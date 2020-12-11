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
        public ConfigTest()
        {
            compilation = CreateCompilation(TestSyntaxes,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                    .WithSpecificDiagnosticOptions(new Dictionary<string, ReportDiagnostic> {
                        {"CS8019",ReportDiagnostic.Suppress },
                    }),
                new[] { expanderCoreReference });
            compilation.SyntaxTrees.Should().HaveCount(TestSyntaxes.Length);
            compilation.GetDiagnostics().Should().BeEmpty();
        }
        private readonly CSharpCompilation compilation;
        private static SyntaxTree[] TestSyntaxes => new[]
        {
                 CSharpSyntaxTree.ParseText(
                    @"using System.Diagnostics;namespace Test{static class Put{public class Nested{ public static void Write(string v){Debug.WriteLine(v);}}}}",
                    path: "/home/source/Put.cs"),
                 CSharpSyntaxTree.ParseText(
                    @"using System.Diagnostics;
using System;
using System.Threading.Tasks;// unused
using System.Collections.Generic;
namespace Test.I
{
    using System.Collections;
    public record IntRecord(int n);
    [System.Diagnostics.DebuggerDisplay(""TEST"")]
    class D<T> : IComparer<T>
    {
        public int Compare(T x,T y) => throw new NotImplementedException();
        [System.Diagnostics.Conditional(""TEST"")]
        public static void WriteType()
        {
            Console.Write(typeof(T).FullName);
            Trace.Write(typeof(T).FullName);
            Put.Nested.Write(typeof(T).FullName);
        }
    }
}",
                    path: "/home/source/I/D.cs"),
                 CSharpSyntaxTree.ParseText(
                   @"using System;
using System.Diagnostics;
using static System.Console;

namespace Test.F
{
    class N
    {
        public static void WriteN()
        {
            Console.Write(NumType.Zero);
            Write(""N"");
            Trace.Write(""N"");
            Put.Nested.Write(""N"");
        }
    }
}",
                    path: "/home/source/F/N.cs"),
                 CSharpSyntaxTree.ParseText(
       @"
using System.Diagnostics;
namespace Test.F
{
    [DebuggerDisplay(""TEST"")]
    public enum NumType
    {
        Zero,
        Pos,
        Neg,
    }
}", path: "/home/source/F/NumType.cs"),
        };
        static readonly CSharpParseOptions opts = new CSharpParseOptions(kind: SourceCodeKind.Regular, documentationMode: DocumentationMode.Parse);
        static readonly ImmutableArray<SourceFileInfo> embeddedFiles = ImmutableArray.Create(
                new SourceFileInfo
                (
                    "TestAssembly>F/N.cs",
                    new string[] { "Test.F.N" },
                    new string[] { "using System;", "using System.Diagnostics;", "using static System.Console;" },
                    new string[] { "TestAssembly>F/NumType.cs", "TestAssembly>Put.cs" },
                    "namespace Test.F{class N{public static void WriteN(){Console.Write(NumType.Zero);Write(\"N\");Trace.Write(\"N\");Put.Nested.Write(\"N\");}}}"
                ), new SourceFileInfo
                (
                    "TestAssembly>F/NumType.cs",
                    new string[] { "Test.F.NumType" },
                    ImmutableArray.Create<string>(),
                    ImmutableArray.Create<string>(),
                    "namespace Test.F{public enum NumType{Zero,Pos,Neg,}}"
                ), new SourceFileInfo
                (
                    "TestAssembly>I/D.cs",
                    new string[] { "Test.I.IntRecord", "Test.I.D<T>" },
                    new string[] { "using System.Diagnostics;", "using System;", "using System.Collections.Generic;" },
                    new string[] { "TestAssembly>Put.cs" },
                      @"namespace Test.I{using System.Collections;public record IntRecord(int n);class D<T> : IComparer<T>{public int Compare(T x, T y) => throw new NotImplementedException();[System.Diagnostics.Conditional(""TEST"")]public static void WriteType(){Console.Write(typeof(T).FullName);Trace.Write(typeof(T).FullName);Put.Nested.Write(typeof(T).FullName);}}}"
                ), new SourceFileInfo
                (
                    "TestAssembly>Put.cs",
                    new string[] { "Test.Put", "Test.Put.Nested" },
                    new string[] { "using System.Diagnostics;" },
                    ImmutableArray.Create<string>(),
                    "namespace Test{static class Put{public class Nested{public static void Write(string v){Debug.WriteLine(v);}}}}"
                ));


        [Fact]
        public void GenerateTest()
        {
            var additionalText = new InMemoryAdditionalText(
                "/foo/bar/SourceExpander.Embedder.Config.json", @"
{
    ""$schema"": ""https://raw.githubusercontent.com/naminodarie/SourceExpander/master/schema/embedder.schema.json"",
    ""exclude-attributes"": [
        ""System.Diagnostics.DebuggerDisplayAttribute""
    ]
}
");

            var generator = new EmbedderGenerator();
            var opts = new CSharpParseOptions(kind: SourceCodeKind.Regular, documentationMode: DocumentationMode.Parse);
            var driver = CSharpGeneratorDriver.Create(
                new[] { generator },
                additionalTexts: new[] { additionalText },
                parseOptions: opts);
            driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);
            diagnostics.Should().BeEmpty();
            outputCompilation.GetDiagnostics().Should().BeEmpty();
            outputCompilation.SyntaxTrees.Should().HaveCount(TestSyntaxes.Length + 1);

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

            outputCompilation.SyntaxTrees.Should().HaveCount(TestSyntaxes.Length + 1);
            diagnostics.Should().BeEmpty();

            outputCompilation.SyntaxTrees
                .Should()
                .ContainSingle(tree => tree.GetRoot(default).ToString().Contains("[assembly: AssemblyMetadataAttribute(\"SourceExpander.EmbeddedSourceCode.GZipBase32768\","))
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
        public void ResolverTest()
        {
            var config = new EmbedderConfig(
                new[] { "System.Diagnostics.DebuggerDisplayAttribute" });
            var reporter = new MockDiagnosticReporter();
            new EmbeddingResolver(compilation, opts, reporter, config).ResolveFiles()
                .Should()
                .BeEquivalentTo(embeddedFiles);
            reporter.Diagnostics.Should().BeEmpty();
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
