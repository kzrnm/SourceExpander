using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace SourceExpander.Generator.Test
{
    public class ExpandGeneratorWithCoreReferenceTest
    {
        [Fact]
        public void GenerateTest()
        {
            var syntaxTrees = new[]
            {
                CSharpSyntaxTree.ParseText(
                    @"using System;
using SampleLibrary;

class Program
{
    static void Main()
    {
        Console.WriteLine(42);
        Put.WriteRandom();
#if !EXPAND_GENERATOR
        Console.WriteLine(24);
#endif
    }
}",
                    options: new CSharpParseOptions(documentationMode:DocumentationMode.None),
                    path: "/home/source/Program.cs"),
                CSharpSyntaxTree.ParseText(
                    @"using System;
using System.Reflection;
using SampleLibrary;
using static System.MathF;
using M = System.Math;

class Program2
{
    static void Main()
    {
        Console.WriteLine(42);
        Put2.Write();
    }
}",
                    options: new CSharpParseOptions(documentationMode:DocumentationMode.None),
                    path: "/home/source/Program2.cs"),
            };

            var sampleReferences = TestUtil.GetSampleDllPaths().Select(path => MetadataReference.CreateFromFile(path));
            var compilation = CSharpCompilation.Create(
                assemblyName: "TestAssembly",
                syntaxTrees: syntaxTrees,
                references: TestUtil.withCoreReferenceMetadatas.Concat(sampleReferences),
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithSpecificDiagnosticOptions(new Dictionary<string, ReportDiagnostic> {
                    { "CS8019", ReportDiagnostic.Suppress },
                }));
            compilation.SyntaxTrees.Should().HaveCount(syntaxTrees.Length);

            var generator = new ExpandGenerator();
            var driver = CSharpGeneratorDriver.Create(new[] { generator }, parseOptions: new CSharpParseOptions(kind: SourceCodeKind.Regular, documentationMode: DocumentationMode.Parse));
            driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);
            diagnostics.Should().BeEmpty();
            outputCompilation.GetDiagnostics().Should().BeEmpty();
            outputCompilation.SyntaxTrees.Should().HaveCount(syntaxTrees.Length + 1);
            outputCompilation.SyntaxTrees
                .Should()
                .ContainSingle(tree => tree.FilePath.EndsWith("SourceExpander.Expanded.cs"))
                .Which
                .ToString()
                .Replace("\r\n", "\n")
                .Replace("\\r\\n", "\\n")
                .Should()
                .Be(File.ReadAllText(TestUtil.GetTestDataPath("wants", "default.test.txt")));
        }
    }
}
