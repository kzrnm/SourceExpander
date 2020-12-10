using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace SourceExpander.Embedder.Generate.Test
{
    public class OtherDependencyTest
    {
        private static CSharpCompilation MakeOtherCompilation(SyntaxTree syntax)
        {
            var compilation = CSharpCompilation.Create("OtherDependency",
                syntaxTrees: new[] {
                        syntax,
                },
                references: TestUtil.defaultMetadatas,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            );

            var generator = new EmbedderGenerator();
            var driver = CSharpGeneratorDriver.Create(new[] { generator }, parseOptions: new CSharpParseOptions(kind: SourceCodeKind.Regular, documentationMode: DocumentationMode.Parse));
            driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out _);
            return (CSharpCompilation)outputCompilation;
        }

        private static CSharpCompilation MakeOldStyleCompilation(SyntaxTree syntax)
        {
            return CSharpCompilation.Create("OtherDependency",
                syntaxTrees: new[] {
                        syntax,
                        CSharpSyntaxTree.ParseText(
                        @"[assembly: System.Reflection.AssemblyMetadata(""SourceExpander.EmbeddedSourceCode"", ""[{\""CodeBody\"":\""namespace Other { public static class C { public static void P() => System.Console.WriteLine(); } } \"",\""Dependencies\"":[],\""FileName\"":\""OtherDependency>C.cs\"",\""TypeNames\"":[\""Other.C\""],\""Usings\"":[]}]"")]",
                        path: @"/home/other/AssemblyInfo.cs")
                },
                references: TestUtil.defaultMetadatas,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            );
        }

        private readonly Dictionary<string, CompilationReference> otherDependencies = new Dictionary<string, CompilationReference>();
        public OtherDependencyTest()
        {
            var syntax = CSharpSyntaxTree.ParseText(
                @"namespace Other{public static class C{public static void P() => System.Console.WriteLine();}}",
                path: @"/home/other/C.cs");

            otherDependencies["current"] = MakeOtherCompilation(syntax).ToMetadataReference();
            otherDependencies["old"] = MakeOldStyleCompilation(syntax).ToMetadataReference();
        }


        [Theory]
        [InlineData("old")]
        [InlineData("current")]
        public void OtherTest(string name)
        {
            var OtherDependency = otherDependencies[name];
            var compilation = CSharpCompilation.Create("Mine",
                syntaxTrees: new[] {
                    CSharpSyntaxTree.ParseText(@"
namespace Mine{
    public static class C
    {
        public static void P() => System.Console.WriteLine();
    }
}", path: @"/home/mine/C.cs"),
                    CSharpSyntaxTree.ParseText(@"
using OC = Other.C;

namespace Mine{
    public static class Program
    {
        public static void Main()
        {
            OC.P();
            C.P();
        }
    }
}", path: @"/home/mine/Program.cs"),
        },
                references: TestUtil.defaultMetadatas.Append(OtherDependency),
                options: new CSharpCompilationOptions(OutputKind.ConsoleApplication));


            compilation.SyntaxTrees.Should().HaveCount(2);
            compilation.GetDiagnostics().Should().BeEmpty();

            var generator = new EmbedderGenerator();
            var opts = new CSharpParseOptions(kind: SourceCodeKind.Regular, documentationMode: DocumentationMode.Parse);
            var driver = CSharpGeneratorDriver.Create(new[] { generator }, parseOptions: opts);
            driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out _);
            outputCompilation.GetDiagnostics().Should().BeEmpty();

            var expected = new[] {
                new SourceFileInfo
                (
                    "Mine>Program.cs",
                    new string[] { "Mine.Program" },
                    new string[] { "using OC = Other.C;" },
                    new string[] { "OtherDependency>C.cs", "Mine>C.cs" },
                    "namespace Mine{public static class Program{public static void Main(){OC.P();C.P();}}}"
                ),
                new SourceFileInfo
                (
                    "Mine>C.cs",
                    new string[] { "Mine.C" },
                    Array.Empty<string>(),
                    Array.Empty<string>(),
                    "namespace Mine{public static class C{public static void P() => System.Console.WriteLine();}}"
                )
            };

            var reporter = new MockDiagnosticReporter();
            new EmbeddingResolver(compilation, opts, reporter, new EmbedderConfig()).ResolveFiles()
                .Should()
                .BeEquivalentTo(expected);

            reporter.Diagnostics.Should().BeEmpty();


            var metadata = outputCompilation.Assembly.GetAttributes()
                .Where(x => x.AttributeClass?.Name == nameof(System.Reflection.AssemblyMetadataAttribute))
                .ToDictionary(x => (string)x.ConstructorArguments[0].Value, x => (string)x.ConstructorArguments[1].Value);
            var embedded = metadata["SourceExpander.EmbeddedSourceCode.GZipBase32768"];
            System.Text.Json.JsonSerializer.Deserialize<SourceFileInfo[]>(
                SourceFileInfoUtil.FromGZipBase32768(embedded))
                .Should()
                .BeEquivalentTo(expected);
        }

        [Fact]
        public void UsingOlderVersion()
        {
            var syntax = CSharpSyntaxTree.ParseText(
                @"namespace Other{public static class C{public static void P() => System.Console.WriteLine();}}",
                path: @"/home/other/C.cs");
            var otherCompilation = MakeOldStyleCompilation(syntax).AddSyntaxTrees(
                CSharpSyntaxTree.ParseText(
                @"[assembly: System.Reflection.AssemblyMetadata(""SourceExpander.EmbedderVersion"",""2147483647.2147483647.2147483647.2147483647"")]",
                path: @"/home/other/AssemblyInfo2.cs"));

            var OtherDependency = otherCompilation.ToMetadataReference();

            var compilation = CSharpCompilation.Create("Mine",
                syntaxTrees: new[] {
                    CSharpSyntaxTree.ParseText(@"
namespace Mine{
    public static class C
    {
        public static void P() => System.Console.WriteLine();
    }
}", path: @"/home/mine/C.cs"),
                    CSharpSyntaxTree.ParseText(@"
using OC = Other.C;

namespace Mine{
    public static class Program
    {
        public static void Main()
        {
            OC.P();
            C.P();
        }
    }
}", path: @"/home/mine/Program.cs"),
        },
                references: TestUtil.defaultMetadatas.Append(OtherDependency),
                options: new CSharpCompilationOptions(OutputKind.ConsoleApplication));


            compilation.SyntaxTrees.Should().HaveCount(2);
            compilation.GetDiagnostics().Should().BeEmpty();

            var generator = new EmbedderGenerator();
            var opts = new CSharpParseOptions(kind: SourceCodeKind.Regular, documentationMode: DocumentationMode.Parse);
            var driver = CSharpGeneratorDriver.Create(new[] { generator }, parseOptions: opts);
            driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out _);
            outputCompilation.GetDiagnostics().Should().BeEmpty();

            var expected = new[] {
                new SourceFileInfo
                (
                    "Mine>Program.cs",
                    new string[] { "Mine.Program" },
                    new string[] { "using OC = Other.C;" },
                    new string[] { "OtherDependency>C.cs", "Mine>C.cs" },
                    "namespace Mine{public static class Program{public static void Main(){OC.P();C.P();}}}"
                ),
                new SourceFileInfo
                (
                    "Mine>C.cs",
                    new string[] { "Mine.C" },
                    Array.Empty<string>(),
                    Array.Empty<string>(),
                    "namespace Mine{public static class C{public static void P() => System.Console.WriteLine();}}"
                )
            };

            var reporter = new MockDiagnosticReporter();
            new EmbeddingResolver(compilation, opts, reporter, new EmbedderConfig()).ResolveFiles()
                .Should()
                .BeEquivalentTo(expected);

            var metadata = outputCompilation.Assembly.GetAttributes()
                        .Where(x => x.AttributeClass?.Name == nameof(System.Reflection.AssemblyMetadataAttribute))
                        .ToDictionary(x => (string)x.ConstructorArguments[0].Value, x => (string)x.ConstructorArguments[1].Value);
            var embedded = metadata["SourceExpander.EmbeddedSourceCode.GZipBase32768"];
            System.Text.Json.JsonSerializer.Deserialize<SourceFileInfo[]>(
                SourceFileInfoUtil.FromGZipBase32768(embedded))
                .Should()
                .BeEquivalentTo(expected);

            var diagnostic = reporter.Diagnostics
                .Should()
                .ContainSingle()
                .Which;
            diagnostic.Id.Should().Be("EMBED0001");
            diagnostic.DefaultSeverity.Should().Be(DiagnosticSeverity.Warning);
            diagnostic.GetMessage()
                .Should()
                .MatchRegex(
                    // language=regex
                    @"embeder version\(\d+\.\d+\.\d+\.\d+\) is older than OtherDependency\(2147483647\.2147483647\.2147483647\.2147483647\)");
        }

    }
}
