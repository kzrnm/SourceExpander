extern alias Generator;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SourceExpander.Expanded;

namespace SourceExpander.Test
{
    public class ExpandTest
    {
        [Fact]
        public void Expand()
        {
            const string code = """
using System;
using SourceExpander;

class Program
{
    static void Main()
    {
        Console.WriteLine(42);
        Expander.Expand();
    }
}
""";
            var syntaxTrees = new[]
            {
                CSharpSyntaxTree.ParseText(
                    code,
                    options: new CSharpParseOptions(documentationMode:DocumentationMode.None)
                        .WithLanguageVersion(LanguageVersion.CSharp4),
                    path: "/home/source/Program.cs"),
            };

            var compilation = CSharpCompilation.Create(
                assemblyName: "TestAssembly",
                syntaxTrees: syntaxTrees,
                references: TestUtil.DefaulMetadatas,
                options: new CSharpCompilationOptions(OutputKind.ConsoleApplication)
                .WithSpecificDiagnosticOptions(new Dictionary<string, ReportDiagnostic> {
                    { "CS8019", ReportDiagnostic.Suppress },
                }));
            compilation.SyntaxTrees.Length.ShouldBe(syntaxTrees.Length);
            compilation.GetDiagnostics().ShouldBeEmpty();

            var generator = new Generator::SourceExpander.ExpandGenerator();
            var driver = CSharpGeneratorDriver.Create(new[] { generator.AsSourceGenerator() },
                parseOptions:
                new CSharpParseOptions(kind: SourceCodeKind.Regular, documentationMode: DocumentationMode.Parse)
                    .WithLanguageVersion(LanguageVersion.CSharp4)
                );
            driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);
            outputCompilation.GetDiagnostics().ShouldBeEmpty();
            outputCompilation.SyntaxTrees.Count().ShouldBe(syntaxTrees.Length + 2);

            outputCompilation.SyntaxTrees
            .Where(tree => tree.FilePath.EndsWith("SourceExpander.Expanded.cs"))
            .ShouldHaveSingleItem();
            var files = TestUtil.GetExpandedFilesWithCore(outputCompilation);
            files.ShouldHaveSingleItem();
            files["/home/source/Program.cs"].ShouldBeEquivalentTo(
                new SourceCode(
                    path: "/home/source/Program.cs",
                    code: """
using SourceExpander;
using System;
using System.Diagnostics;
class Program
{
    static void Main()
    {
        Console.WriteLine(42);
        Expander.Expand();
    }
}
#region Expanded by https://github.com/kzrnm/SourceExpander
namespace SourceExpander{public class Expander{[Conditional("EXP")]public static void Expand(string inputFilePath=null,string outputFilePath=null,bool ignoreAnyError=true){}public static string ExpandString(string inputFilePath=null,bool ignoreAnyError=true){return "";}}}
#endregion Expanded by https://github.com/kzrnm/SourceExpander
""".ReplaceEOL())
                );
        }
    }
}
