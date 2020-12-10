using System.Collections.Immutable;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using static SourceExpander.Embedder.TestUtil;

namespace SourceExpander.Embedder.Generate.Test
{
    public class NoSyntaxTest
    {
        [Fact]
        public void GenerateNoSyntaxesTest()
        {
            var compilation = CSharpCompilation.Create(
                assemblyName: "TestAssembly",
                syntaxTrees: ImmutableArray.Create<SyntaxTree>(),
                references: defaultMetadatas,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            compilation.SyntaxTrees.Should().BeEmpty();
            compilation.GetDiagnostics()
                .Should().BeEmpty();

            var generator = new EmbedderGenerator();
            var driver = CSharpGeneratorDriver.Create(new[] { generator }, parseOptions: new CSharpParseOptions(kind: SourceCodeKind.Regular, documentationMode: DocumentationMode.Parse));
            driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);
            outputCompilation.SyntaxTrees.Should().BeEmpty();
            diagnostics.Should().BeEmpty();
            outputCompilation.GetDiagnostics().Should().BeEmpty();
        }
    }
}
