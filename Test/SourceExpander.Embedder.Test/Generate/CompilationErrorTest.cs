using System.Collections.Immutable;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace SourceExpander.Embedder.Generate.Test
{
    public class CompilationErrorTest : EmbeddingGeneratorTestBase
    {
        static readonly CSharpParseOptions opts = new CSharpParseOptions(kind: SourceCodeKind.Regular, documentationMode: DocumentationMode.Parse);
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
            var driver = CSharpGeneratorDriver.Create(new[] { generator }, parseOptions: new CSharpParseOptions(kind: SourceCodeKind.Regular, documentationMode: DocumentationMode.Parse));
            driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);
            outputCompilation.SyntaxTrees.Should().ContainSingle();
            diagnostics.Should().BeEmpty();
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
            new EmbeddingResolver(compilation, opts, reporter, new EmbedderConfig()).ResolveFiles().Should().ContainSingle();

            var diagnostic = reporter.Diagnostics.Should().ContainSingle().Which;
            diagnostic.Id.Should().Be("EMBED0004");
            diagnostic.DefaultSeverity.Should().Be(DiagnosticSeverity.Error);
            diagnostic.GetMessage().Should().Be("Error embedded source: TestAssembly>Program.cs");
        }
    }
}
