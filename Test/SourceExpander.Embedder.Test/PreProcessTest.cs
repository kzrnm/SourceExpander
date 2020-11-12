extern alias Core;

using System;
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Newtonsoft.Json;
using Xunit;
using static SourceExpander.Embedder.Test.Util;

namespace SourceExpander.Embedder.Test
{
    public class PreProcessTest
    {
        [Fact]
        public void GenerateTest()
        {
            var compilation = CSharpCompilation.Create(
                assemblyName: "TestAssembly",
                syntaxTrees: new[] { CSharpSyntaxTree.ParseText(@"using System;
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
options: new CSharpParseOptions(preprocessorSymbols:new[]{ "Trace" }),
path: "Program.cs") },
                references: defaultMetadatas.Append(expanderCoreReference),
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            compilation.SyntaxTrees.Should().HaveCount(1);
            compilation.GetDiagnostics().Should().BeEmpty();

            var generator = new EmbeddedGenerator();
            var driver = CSharpGeneratorDriver.Create(new[] { generator }, parseOptions: new CSharpParseOptions(kind: SourceCodeKind.Regular, documentationMode: DocumentationMode.Parse));
            driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);
            diagnostics.Should().BeEmpty();
            outputCompilation.SyntaxTrees.Should().HaveCount(2);

            var newTree = outputCompilation.SyntaxTrees
                .Should()
                .ContainSingle(tree => tree.FilePath.Contains("EmbeddedSourceCode.Metadata.Generated.cs"))
                .Which;
            newTree.ToString().Should().StartWith("[assembly: System.Reflection.AssemblyMetadataAttribute(\"SourceExpander.EmbeddedSourceCode\", ");
            newTree.GetDiagnostics().Should().BeEmpty();
        }
    }
}
