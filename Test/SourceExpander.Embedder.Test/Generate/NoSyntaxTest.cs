using System.Collections.Immutable;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace SourceExpander.Embedder.Generate.Test
{
    public class NoSyntaxTest : EmbeddingGeneratorTestBase
    {
        public NoSyntaxTest()
        {
            compilation = CreateCompilation(ImmutableArray.Create<SyntaxTree>(),
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            compilation.SyntaxTrees.Should().BeEmpty();
            compilation.GetDiagnostics()
                .Should().BeEmpty();
        }
        private readonly CSharpCompilation compilation;
        static readonly CSharpParseOptions parseOptions = new CSharpParseOptions(kind: SourceCodeKind.Regular, documentationMode: DocumentationMode.Parse);
        [Fact]
        public void GenerateNoSyntaxesTest()
        {
            var generator = new EmbedderGenerator();
            var gen = RunGenerator(compilation, generator, parseOptions: parseOptions);
            gen.Diagnostics.Should().BeEmpty();
            gen.OutputCompilation.SyntaxTrees.Should().BeEmpty();
            gen.OutputCompilation.GetDiagnostics().Should().BeEmpty();
        }

        [Fact]
        public void ResolverTest()
        {
            var reporter = new MockDiagnosticReporter();
            new EmbeddingResolver(compilation, parseOptions, reporter, new EmbedderConfig()).ResolveFiles()
                .Should().BeEmpty();
            reporter.Diagnostics.Should().BeEmpty();
        }
    }
}
