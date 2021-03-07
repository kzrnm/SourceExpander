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
            gen.OutputCompilation.SyntaxTrees.Should().HaveCount(2 + CompileTimeTypeMaker.SourceCount);
        }

        [Fact]
        public void ResolverOutputError()
        {
            var compilation = CreateCompilation(ImmutableArray.Create(
                CSharpSyntaxTree.ParseText(@"
class Program
{
    public static int Method() => 1 + 2 ** 3;
}
", path: "/home/test/Program.cs")),
                     new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            var reporter = new MockDiagnosticReporter();
            new EmbeddingResolver(compilation, parseOptions, reporter, new EmbedderConfig())
                .ResolveFiles()
                .Should()
                .BeEquivalentTo(new SourceFileInfo(
                    fileName: "TestAssembly>Program.cs",
                    typeNames: ImmutableArray.Create("Program"),
                    usings: ImmutableArray<string>.Empty,
                    dependencies: ImmutableArray<string>.Empty,
                    codeBody: "class Program { public static int Method() => 1 + 2 * *3; }"
                    )
                );
        }
    }
}
