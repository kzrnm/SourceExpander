using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace SourceExpander.Generator.Test
{
    public class ExpandGeneratorTest
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
#end if
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
        Put.WriteRandom();
    }
}",
                    options: new CSharpParseOptions(documentationMode:DocumentationMode.None),
                    path: "/home/source/Program2.cs"),
            };

            var sampleReference = MetadataReference.CreateFromFile(GetSampleDllPath());
            var compilation = CSharpCompilation.Create(
                assemblyName: "TestAssembly",
                syntaxTrees: syntaxTrees,
                references: defaultMetadatas.Append(sampleReference),
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithSpecificDiagnosticOptions(new Dictionary<string, ReportDiagnostic> {
                    { "CS8019", ReportDiagnostic.Suppress },
                }));
            compilation.SyntaxTrees.Should().HaveCount(syntaxTrees.Length);

            var generator = new ExpandGenerator();
            var driver = CSharpGeneratorDriver.Create(new[] { generator }, parseOptions: new CSharpParseOptions(kind: SourceCodeKind.Regular, documentationMode: DocumentationMode.Parse));
            driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);
            diagnostics.Should().BeEmpty();
            outputCompilation.SyntaxTrees.Should().HaveCount(syntaxTrees.Length + 1);
            outputCompilation.SyntaxTrees
                .Single(tree => tree.FilePath.EndsWith("SourceExpander.Expanded.cs"))
                .ToString()
                .Should()
                .NotContain("System.Reflection")
                .And
                .NotContain("System.Math");
        }
        [Fact]
        public void GenerateNotFoundTest()
        {
            var syntaxTrees = new[]
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
            };

            var sampleReference = MetadataReference.CreateFromFile(GetSampleDllPath());
            var compilation = CSharpCompilation.Create(
                assemblyName: "TestAssembly",
                syntaxTrees: syntaxTrees,
                references: defaultMetadatas,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithSpecificDiagnosticOptions(new Dictionary<string, ReportDiagnostic> {
                    { "CS8019", ReportDiagnostic.Suppress },
                }));
            compilation.SyntaxTrees.Should().HaveCount(syntaxTrees.Length);

            var generator = new ExpandGenerator();
            var driver = CSharpGeneratorDriver.Create(new[] { generator }, parseOptions: new CSharpParseOptions(kind: SourceCodeKind.Regular, documentationMode: DocumentationMode.Parse));
            driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);
            diagnostics.Should().ContainSingle()
                .Which
                .Id
                .Should().Be("EXPAND0001");
            outputCompilation.SyntaxTrees.Should().HaveCount(syntaxTrees.Length + 1);
            outputCompilation.SyntaxTrees
                .Single(tree => tree.FilePath.EndsWith("SourceExpander.Expanded.cs"))
                .ToString()
                .Should()
                .NotContain("System.Reflection")
                .And
                .NotContain("System.Math");
        }

        static string GetSampleDllPath()
            => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "SampleLibrary.dll");

        static readonly MetadataReference[] defaultMetadatas = GetDefaulMetadatas().ToArray();
        static IEnumerable<MetadataReference> GetDefaulMetadatas()
        {
            var directory = Path.GetDirectoryName(typeof(object).Assembly.Location);
            foreach (var file in Directory.EnumerateFiles(directory, "System*.dll"))
            {
                yield return MetadataReference.CreateFromFile(file);
            }
        }
    }
}
