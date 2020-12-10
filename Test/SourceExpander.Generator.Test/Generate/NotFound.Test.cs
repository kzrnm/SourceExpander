using System.Collections.Generic;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SourceExpander.Expanded;
using Xunit;

namespace SourceExpander.Generator.Generate.Test
{
    public class NotFoundTest
    {
        [Fact]
        public void GenerateTest()
        {
            const string code = @"using System;
using System.Reflection;
using static System.Math;

class Program
{
    static void Main()
    {
        Console.WriteLine(42);
    }
}";
            var syntaxTrees = new[]
            {
                CSharpSyntaxTree.ParseText(
                    code,
                    options: new CSharpParseOptions(documentationMode:DocumentationMode.None),
                    path: "/home/source/Program.cs"),
            };

            var compilation = CSharpCompilation.Create(
                assemblyName: "TestAssembly",
                syntaxTrees: syntaxTrees,
                references: TestUtil.withCoreReferenceMetadatas,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithSpecificDiagnosticOptions(new Dictionary<string, ReportDiagnostic> {
                    { "CS8019", ReportDiagnostic.Suppress },
                }));
            compilation.SyntaxTrees.Should().HaveCount(syntaxTrees.Length);

            var generator = new ExpandGenerator();
            var driver = CSharpGeneratorDriver.Create(new[] { generator }, parseOptions: new CSharpParseOptions(kind: SourceCodeKind.Regular, documentationMode: DocumentationMode.Parse));
            driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);
            outputCompilation.GetDiagnostics().Should().BeEmpty();
            diagnostics.Should().ContainSingle()
                .Which
                .Id
                .Should().Be("EXPAND0001");
            outputCompilation.SyntaxTrees.Should().HaveCount(syntaxTrees.Length + 1);

            outputCompilation.SyntaxTrees
            .Should()
            .ContainSingle(tree => tree.FilePath.EndsWith("SourceExpander.Expanded.cs"));
            var files = TestUtil.GetExpandedFilesWithCore(outputCompilation);
            files.Should().HaveCount(1);
            files["/home/source/Program.cs"].Should()
                .BeEquivalentTo(
                new SourceCode(
                    path: "/home/source/Program.cs",
                    code: @"using System;
class Program
{
    static void Main()
    {
        Console.WriteLine(42);
    }
}
#region Expanded
#endregion Expanded
")
                );
        }
    }
}
