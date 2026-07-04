using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;

namespace SourceExpander.Generate;

public class PreProcessTest : EmbedderGeneratorTestBase
{
    [Test]
    public async Task Generate(CancellationToken cancellationToken)
    {
        const string embeddedSourceCode = """[{"CodeBody":"class Program{int[]v=new int[]{2,4,6,};static void Main()=>Console.WriteLine(1);}","Dependencies":[],"FileName":"TestProject>Program.cs","TypeNames":["Program"],"Usings":["using System;"]}]""";

        await embeddedSourceCode.Should().BeEquivalentToJsonSources([
             new SourceFileInfo
             (
                 "TestProject>Program.cs",
                 ["Program"],
                 ["using System;"],
                 [],
                 "class Program{int[]v=new int[]{2,4,6,};static void Main()=>Console.WriteLine(1);}"
             ),
        ]);

        var test = new Test(new()
        {
            EmbeddedSourceCode = embeddedSourceCode,
        })
        {
            ParseOptions = new CSharpParseOptions(
                LanguageVersion.Preview,
                kind: SourceCodeKind.Regular,
                documentationMode: DocumentationMode.Parse,
                preprocessorSymbols: ["Trace", "TEST"]),
            TestState =
            {
                AdditionalFiles =
                {
                    enableMinifyJson,
                },
                Sources = {
                    (
                        "/home/source/Program.cs",
// lang=C#
"""
using System;
class Program
{
    int[] v = new int[]
    {
#if SOURCE_EMBEDDINGA
    1,
#elif SOURCE_EMBEDDING
    2,
#else
    0,
#endif
#if !SOURCE_EMBEDDING
    3,
#else
    4,
#endif
#if SOURCE_EMBEDDING && NOT_DEFINED
    5,
#endif
#if NOT_DEFINED || SOURCE_EMBEDDING
    6,
#endif
    };
    static void Main() =>
#if TRACE
    Console.WriteLine(0);
#else
    Console.WriteLine(
#if TEST
1
#else
2
#endif
);
#endif
}
"""
                    ),
                },
            }
        };
        await test.RunAsync(cancellationToken);
    }

    [Test]
    public async Task WithError(CancellationToken cancellationToken)
    {
        const string embeddedSourceCode = """[{"CodeBody":"class Program{[M(256)]static void Do()=>WriteLine(0);}","Dependencies":[],"FileName":"TestProject>Program.cs","TypeNames":["Program"],"Usings":["using M = System.Runtime.CompilerServices.MethodImplAttribute;","using static System.Console;"]}]""";

        await embeddedSourceCode.Should().BeEquivalentToJsonSources([
             new SourceFileInfo
             (
                 "TestProject>Program.cs",
                 ["Program"],
                 ["using M = System.Runtime.CompilerServices.MethodImplAttribute;","using static System.Console;"],
                 [],
                 "class Program{[M(256)]static void Do()=>WriteLine(0);}"
             ),
        ]);

        var test = new Test(new()
        {
            EmbeddedSourceCode = embeddedSourceCode,
        })
        {
            ParseOptions = new CSharpParseOptions(
                LanguageVersion.Preview,
                kind: SourceCodeKind.Regular,
                documentationMode: DocumentationMode.Parse
            ),
            ExpectedDiagnostics =
            {
                new DiagnosticResult("EMBED0009", DiagnosticSeverity.Info).WithSpan("/home/source/Program.cs", 3, 1, 3, 29),
                new DiagnosticResult("EMBED0010", DiagnosticSeverity.Info).WithSpan("/home/source/Program.cs", 4, 1, 4, 63),
            },
            TestState =
            {
                AdditionalFiles =
                {
                    enableMinifyJson,
                },
                Sources = {
                    (
                        "/home/source/Program.cs",
// lang=C#
"""
using System;
#if SOURCE_EMBEDDING
using static System.Console;
using M = System.Runtime.CompilerServices.MethodImplAttribute;
#endif
class Program
{
#if SOURCE_EMBEDDING
    [M(256)]
#endif
    static void Do() =>
#if !SOURCE_EMBEDDING
    Console.
#endif
    WriteLine(0);

    [SourceExpander.NotEmbeddingSource]
    static void NotEmbedding() => Console.WriteLine(0);

#if SOURCE_EMBEDDING
    [SourceExpander.NotEmbeddingSource]
#endif
    static void NotEmbedding2() => Console.WriteLine(0);
}
"""
                    ),
                },
            }
        };
        await test.RunAsync(cancellationToken);
    }
}
