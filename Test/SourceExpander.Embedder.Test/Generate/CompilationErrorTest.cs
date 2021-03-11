using System.Collections.Immutable;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace SourceExpander.Embedder.Generate.Test
{
    public class CompilationErrorTest : EmbeddingGeneratorTestBase
    {
        static readonly CSharpParseOptions parseOptions = new(kind: SourceCodeKind.Regular, documentationMode: DocumentationMode.Parse);
        [Fact]
        public void GenerateInputError()
        {
            var compilation = CreateCompilation(ImmutableArray.Create(
                CSharpSyntaxTree.ParseText(@"
class Program
{
    public static int Method() => 1 + 2 ** 3;
}
", path: "/home/test/Program.cs")),
                     new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            var generator = new EmbedderGenerator();
            var gen = RunGenerator(compilation, generator, parseOptions: parseOptions);
            gen.OutputCompilation.SyntaxTrees.Should().HaveCount(1 + CompileTimeTypeMaker.SourceCount);
        }
    }
}
