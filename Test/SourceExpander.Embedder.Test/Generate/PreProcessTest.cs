using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

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
                 ImmutableArray.Create("using System;"),
                 ImmutableArray<string>.Empty,
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
                preprocessorSymbols: new[] { "Trace", "TEST" }),
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
}
