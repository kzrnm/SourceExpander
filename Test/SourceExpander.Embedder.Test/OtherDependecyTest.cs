using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace SourceExpander.Embedder.Test
{
    public class OtherDependecyTest
    {
        private readonly Dictionary<string, CompilationReference> otherDependecies = new Dictionary<string, CompilationReference>();
        public OtherDependecyTest()
        {
            var syntax = CSharpSyntaxTree.ParseText(
                @"namespace Other{public static class C{public static void P() => System.Console.WriteLine();}}",
                path: @"/home/other/C.cs");

            {
                var compilation = CSharpCompilation.Create("OtherDependecy",
                    syntaxTrees: new[] {
                        syntax,
                    },
                    references: Util.defaultMetadatas,
                    options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                );

                var generator = new EmbeddedGenerator();
                var driver = CSharpGeneratorDriver.Create(new[] { generator }, parseOptions: new CSharpParseOptions(kind: SourceCodeKind.Regular, documentationMode: DocumentationMode.Parse));
                driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out _);
                otherDependecies["current"] = outputCompilation.ToMetadataReference();
            }
            {
                var compilation = CSharpCompilation.Create("OtherDependecy",
                    syntaxTrees: new[] {
                        syntax,
                        CSharpSyntaxTree.ParseText(
                        @"[assembly: System.Reflection.AssemblyMetadata(""SourceExpander.EmbeddedSourceCode"", ""[{\""CodeBody\"":\""namespace Other { public static class C { public static void P() => System.Console.WriteLine(); } } \"",\""Dependencies\"":[],\""FileName\"":\""OtherDependecy>C.cs\"",\""TypeNames\"":[\""Other.C\""],\""Usings\"":[]}]"")]",
                        path: @"/home/other/AssemblyInfo.cs")
                    },
                    references: Util.defaultMetadatas,
                    options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                );
                otherDependecies["old"] = compilation.ToMetadataReference();
            }
        }


        [Theory]
        [InlineData("old")]
        [InlineData("current")]
        public void OtherTest(string name)
        {
            var otherDependecy = otherDependecies[name];
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
                references: Util.defaultMetadatas.Append(otherDependecy),
                options: new CSharpCompilationOptions(OutputKind.ConsoleApplication));


            compilation.SyntaxTrees.Should().HaveCount(2);
            compilation.GetDiagnostics().Should().BeEmpty();

            var generator = new EmbeddedGenerator();
            var driver = CSharpGeneratorDriver.Create(new[] { generator }, parseOptions: new CSharpParseOptions(kind: SourceCodeKind.Regular, documentationMode: DocumentationMode.Parse));
            driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out _);

            generator.ResolveFiles(compilation)
                .Should()
                .BeEquivalentTo(
                new SourceFileInfo
                {
                    FileName = "Mine>Program.cs",
                    TypeNames = new string[] { "Mine.Program" },
                    Usings = new string[] { "using OC = Other.C;" },
                    Dependencies = new string[] { "OtherDependecy>C.cs", "Mine>C.cs" },
                    CodeBody = "namespace Mine{ public static class Program { public static void Main() { OC.P(); C.P(); } } }",
                },
                new SourceFileInfo
                {
                    FileName = "Mine>C.cs",
                    TypeNames = new string[] { "Mine.C" },
                    Usings = Array.Empty<string>(),
                    Dependencies = Array.Empty<string>(),
                    CodeBody = "namespace Mine{ public static class C { public static void P() => System.Console.WriteLine(); } }",
                });
        }
    }
}
