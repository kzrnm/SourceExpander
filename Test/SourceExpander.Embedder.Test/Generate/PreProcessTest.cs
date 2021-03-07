using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace SourceExpander.Embedder.Generate.Test
{
    public class PreProcessTest : EmbeddingGeneratorTestBase
    {
        static readonly CSharpParseOptions parseOptions = new(kind: SourceCodeKind.Regular, documentationMode: DocumentationMode.Parse);

        [Fact]
        public void GenerateTest()
        {
            var compilation = CreateCompilation(
                new[] { CSharpSyntaxTree.ParseText(@"using System;
class Program
{
    static void Main() =>
#if TRACE
    Console.WriteLine(0);
#else
    Console.WriteLine(1);
#endif
}
",
new CSharpParseOptions(preprocessorSymbols:new[]{ "Trace" }),
path: "Program.cs") },
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
                additionalMetadatas: new[] { expanderCoreReference });
            compilation.SyntaxTrees.Should().HaveCount(1);
            compilation.GetDiagnostics().Should().BeEmpty();

            var generator = new EmbedderGenerator();
            var gen = RunGenerator(compilation, generator, parseOptions: parseOptions);
            gen.Diagnostics.Should().BeEmpty();
            gen.OutputCompilation.GetDiagnostics().Should().BeEmpty();
            gen.OutputCompilation.SyntaxTrees.Should().HaveCount(3);

            var newTree = gen.AddedSyntaxTrees
                .Should()
                .ContainSingle(t => t.FilePath.EndsWith("EmbeddedSourceCode.Metadata.cs"))
                .Which;
            newTree.ToString().Should().ContainAll(
                "[assembly: AssemblyMetadataAttribute(\"SourceExpander.EmbeddedSourceCode.GZipBase32768\",",
                "[assembly: AssemblyMetadataAttribute(\"SourceExpander.EmbedderVersion\","
                );
            newTree.GetDiagnostics().Should().BeEmpty();
        }
    }
}
