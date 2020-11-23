using System.Collections.Generic;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace SourceExpander.Generator.Test
{
    public class ExpandGeneratorNotFoundTest
    {
        [Fact]
        public void GenerateNotFoundTest()
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
                references: TestUtil.defaultMetadatas,
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
            .Should()
            .ContainSingle(tree => tree.FilePath.EndsWith("SourceExpander.Expanded.cs"))
            .Which
            .ToString()
            .Replace("\r\n", "\n")
            .Replace("\\r\\n", "\\n")
            .Should()
            .Be("using System.Collections.Generic;\n" +
                "namespace SourceExpander.Expanded{\n" +
                    "public class SourceCode{ public string Path; public string Code; }\n" +
                    "public static class Expanded{\n" +
                    "public static IReadOnlyDictionary<string, SourceCode> Files { get; } = new Dictionary<string, SourceCode>{\n" +
                    "{\"/home/source/Program.cs\", new SourceCode{ Path=\"/home/source/Program.cs\", Code=\"using System;\\n" +
                    "class Program\\n{\\n    static void Main()\\n    {\\n        Console.WriteLine(42);\\n    }\\n}\\n#region Expanded\\n#endregion Expanded\\n\" } },\n" +
                "};\n" +
            "}}\n");
        }
    }
}
