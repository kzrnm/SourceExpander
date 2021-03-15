using System.Collections.Generic;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SourceExpander.Expanded;
using Xunit;

namespace SourceExpander.Generator.Generate.Test
{
    public class NotFoundTest : ExpandGeneratorTestBase
    {
        [Fact]
        public void GenerateTest()
        {
            var compilation = CreateCompilation(
                new[]
                {
                    CSharpSyntaxTree.ParseText(
                        @"using System;
using System.Reflection;
using static System.Math;

class Program
{
    static void Main()
    {
        Console.WriteLine(42);
    }
}",
                        options: new CSharpParseOptions(documentationMode:DocumentationMode.None),
                        path: "/home/source/Program.cs"),
                },
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                    .WithSpecificDiagnosticOptions(new Dictionary<string, ReportDiagnostic> {
                        { "CS8019", ReportDiagnostic.Suppress },
                    }),
                additionalMetadatas: new[] { coreReference });
            compilation.SyntaxTrees.Should().HaveCount(1);

            var generator = new ExpandGenerator();
            var gen = RunGenerator(compilation, generator,
                parseOptions: new CSharpParseOptions(kind: SourceCodeKind.Regular, documentationMode: DocumentationMode.Parse));
            gen.OutputCompilation.GetDiagnostics().Should().BeEmpty();
            gen.Diagnostics.Should().ContainSingle()
                .Which
                .Id
                .Should().Be("EXPAND0003");
            gen.OutputCompilation.SyntaxTrees.Should().HaveCount(2);

            gen.OutputCompilation.SyntaxTrees
            .Should()
            .ContainSingle(tree => tree.FilePath.EndsWith("SourceExpander.Expanded.cs"));
            var files = GetExpandedFilesWithCore(gen.OutputCompilation);
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
#region Expanded by https://github.com/naminodarie/SourceExpander
#endregion Expanded by https://github.com/naminodarie/SourceExpander
")
                );
        }
    }
}
