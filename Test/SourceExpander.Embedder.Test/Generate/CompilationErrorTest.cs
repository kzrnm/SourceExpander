using System.Collections.Immutable;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace SourceExpander.Embedder.Generate.Test
{
    public class CompilationErrorTest : EmbeddingGeneratorTestBase
    {
        static readonly CSharpParseOptions parseOptions = new CSharpParseOptions(kind: SourceCodeKind.Regular, documentationMode: DocumentationMode.Parse);
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
            gen.OutputCompilation.SyntaxTrees.Should().ContainSingle();
            gen.Diagnostics.Should().BeEmpty();
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
            new EmbeddingResolver(compilation, parseOptions, reporter, new EmbedderConfig()).ResolveFiles().Should().ContainSingle();

            var diagnostic = reporter.Diagnostics.Should().ContainSingle().Which;
            diagnostic.Id.Should().Be("EMBED0004");
            diagnostic.DefaultSeverity.Should().Be(DiagnosticSeverity.Error);
            diagnostic.GetMessage().Should().Be("Error embedded source: TestAssembly>Program.cs");
        }
    }
}
