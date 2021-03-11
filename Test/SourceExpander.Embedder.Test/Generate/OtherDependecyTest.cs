using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace SourceExpander.Embedder.Generate.Test
{
    public class OtherDependencyTest : EmbeddingGeneratorTestBase
    {
        private static CSharpCompilation MakeOtherCompilation(SyntaxTree syntax)
        {
            var compilation = CreateCompilation(
                new[] { syntax, },
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
                assemblyName: "OtherDependency");

            var generator = new EmbedderGenerator();
            return RunGenerator(compilation, generator,
                parseOptions: new CSharpParseOptions(kind: SourceCodeKind.Regular, documentationMode: DocumentationMode.Parse))
                .OutputCompilation;
        }

        private static CSharpCompilation MakeOldStyleCompilation(SyntaxTree syntax)
        {
            return CreateCompilation(new[] {
                        syntax,
                        CSharpSyntaxTree.ParseText(
                        @"[assembly: System.Reflection.AssemblyMetadata(""SourceExpander.EmbeddedSourceCode"", ""[{\""CodeBody\"":\""namespace Other { public static class C { public static void P() => System.Console.WriteLine(); } } \"",\""Dependencies\"":[],\""FileName\"":\""OtherDependency>C.cs\"",\""TypeNames\"":[\""Other.C\""],\""Usings\"":[]}]"")]",
                        path: @"/home/other/AssemblyInfo.cs")
                },
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
                assemblyName: "OtherDependency");
        }

        private readonly Dictionary<string, CompilationReference> otherDependencies = new();
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
            var compilation = CreateCompilation(
                new[] {
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
                new CSharpCompilationOptions(OutputKind.ConsoleApplication),
                new[] { otherDependencies[name] }
                );
            compilation.SyntaxTrees.Should().HaveCount(2);
            compilation.GetDiagnostics().Should().BeEmpty();

            var generator = new EmbedderGenerator();
            var parseOptions = new CSharpParseOptions(kind: SourceCodeKind.Regular, documentationMode: DocumentationMode.Parse);
            var gen = RunGenerator(compilation, generator, additionalTexts: new[] { enableMinifyJson }, parseOptions: parseOptions);
            gen.OutputCompilation.GetDiagnostics().Should().BeEmpty();

            var expected = new[] {
                new SourceFileInfo
                (
                    "TestAssembly>Program.cs",
                    new string[] { "Mine.Program" },
                    new string[] { "using OC = Other.C;" },
                    new string[] { "OtherDependency>C.cs", "TestAssembly>C.cs" },
                    "namespace Mine{public static class Program{public static void Main(){OC.P();C.P();}}}"
                ),
                new SourceFileInfo
                (
                    "TestAssembly>C.cs",
                    new string[] { "Mine.C" },
                    Array.Empty<string>(),
                    Array.Empty<string>(),
                    "namespace Mine{public static class C{public static void P()=>System.Console.WriteLine();}}"
                )
            };

            var metadata = gen.OutputCompilation.Assembly.GetAttributes()
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

            var otherDependency = otherCompilation.ToMetadataReference();

            var compilation = CreateCompilation(
                new[] {
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
                new CSharpCompilationOptions(OutputKind.ConsoleApplication),
                additionalMetadatas: new[] { otherDependency }
                );

            compilation.SyntaxTrees.Should().HaveCount(2);
            compilation.GetDiagnostics().Should().BeEmpty();

            var generator = new EmbedderGenerator();
            var parseOptions = new CSharpParseOptions(kind: SourceCodeKind.Regular, documentationMode: DocumentationMode.Parse);
            var gen = RunGenerator(compilation, generator, additionalTexts: new[] { enableMinifyJson }, parseOptions: parseOptions);
            gen.OutputCompilation.GetDiagnostics().Should().BeEmpty();

            var expected = new[] {
                new SourceFileInfo
                (
                    "TestAssembly>Program.cs",
                    new string[] { "Mine.Program" },
                    new string[] { "using OC = Other.C;" },
                    new string[] { "OtherDependency>C.cs", "TestAssembly>C.cs" },
                    "namespace Mine{public static class Program{public static void Main(){OC.P();C.P();}}}"
                ),
                new SourceFileInfo
                (
                    "TestAssembly>C.cs",
                    new string[] { "Mine.C" },
                    Array.Empty<string>(),
                    Array.Empty<string>(),
                    "namespace Mine{public static class C{public static void P()=>System.Console.WriteLine();}}"
                )
            };

            var metadata = gen.OutputCompilation.Assembly.GetAttributes()
                        .Where(x => x.AttributeClass?.Name == nameof(System.Reflection.AssemblyMetadataAttribute))
                        .ToDictionary(x => (string)x.ConstructorArguments[0].Value, x => (string)x.ConstructorArguments[1].Value);
            var embedded = metadata["SourceExpander.EmbeddedSourceCode.GZipBase32768"];
            System.Text.Json.JsonSerializer.Deserialize<SourceFileInfo[]>(
                SourceFileInfoUtil.FromGZipBase32768(embedded))
                .Should()
                .BeEquivalentTo(expected);

            var diagnostic = gen.Diagnostics
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
