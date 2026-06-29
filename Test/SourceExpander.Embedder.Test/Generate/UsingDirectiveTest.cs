using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;

namespace SourceExpander.Generate;

public class UsingDirectiveTest : EmbedderGeneratorTestBase
{
    [Test]
    public async Task Generate(CancellationToken cancellationToken)
    {
        const string embeddedSourceCode = "[{\"CodeBody\":\"class Program{static void Main()=>Console.WriteLine(0);static List L(int x,int y)=>new List(Min(x,y));}\",\"Dependencies\":[],\"FileName\":\"TestProject>Program.cs\",\"TypeNames\":[\"Program\"],\"Usings\":[\"using List = System.Collections.Generic.List<int>;\",\"using System;\",\"using static System.Math;\"]}]";
        await embeddedSourceCode.Should().BeEquivalentToJsonSources([
             new SourceFileInfo
             (
                 "TestProject>Program.cs",
                 ["Program"],
                 ImmutableArray.Create("using List = System.Collections.Generic.List<int>;", "using System;", "using static System.Math;"),
                 ImmutableArray<string>.Empty,
                 "class Program{static void Main()=>Console.WriteLine(0);static List L(int x,int y)=>new List(Min(x,y));}"
             )
        ]);

        var test = new Test(new()
        {
            EmbeddedSourceCode = embeddedSourceCode,
        })
        {
            TestState =
            {
                AdditionalFiles = { enableMinifyJson },
                Sources = {
                    (
                        "/home/source/Program.cs",
                        """
using System;
using static System.Math;
using List = System.Collections.Generic.List<int>;
class Program
{
    static void Main() => Console.WriteLine(0);
    static List L(int x, int y) => new List(Min(x, y));
}
"""
                    ),
                },
                ExpectedDiagnostics =
                {
                    new DiagnosticResult("EMBED0009", DiagnosticSeverity.Info).WithSpan("/home/source/Program.cs", 2, 1, 2, 26),
                    new DiagnosticResult("EMBED0010", DiagnosticSeverity.Info).WithSpan("/home/source/Program.cs", 3, 1, 3, 51),

                },
            }
        };
        await test.RunAsync(cancellationToken);
    }
}
