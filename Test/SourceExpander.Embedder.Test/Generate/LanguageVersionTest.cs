using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace SourceExpander.Generate;

public class LanguageVersionTest : EmbedderGeneratorTestBase
{
    [Test]
    [Arguments(LanguageVersion.Latest)]
    [Arguments(LanguageVersion.LatestMajor)]
    [Arguments(LanguageVersion.Preview)]
    [Arguments(LanguageVersion.CSharp7_3)]
    [Arguments(LanguageVersion.CSharp8)]
    [Arguments(LanguageVersion.CSharp9)]
    [Arguments(LanguageVersion.CSharp10)]
    public async Task Generate(LanguageVersion languageVersion, CancellationToken cancellationToken)
    {
        const string embeddedSourceCode = "[{\"CodeBody\":\"class Program{static void Main()=>Console.WriteLine(1);}\",\"Dependencies\":[],\"FileName\":\"TestProject>Program.cs\",\"TypeNames\":[\"Program\"],\"Usings\":[\"using System;\"]}]";
        await embeddedSourceCode.Should().BeEquivalentToJsonSources([
             new SourceFileInfo
             (
                 "TestProject>Program.cs",
                 ["Program"],
                 ImmutableArray.Create("using System;"),
                 ImmutableArray<string>.Empty,
                 """class Program{static void Main()=>Console.WriteLine(1);}"""
             )
        ]);

        var test = new Test(new()
        {
            EmbeddedLanguageVersion = languageVersion.MapSpecifiedToEffectiveVersion().ToDisplayString(),
            EmbeddedSourceCode = embeddedSourceCode,
        })
        {
            ParseOptions = new CSharpParseOptions(languageVersion),
            TestState =
            {
                AdditionalFiles =
                {
                    enableMinifyJson,
                },
                Sources = {
                    (
                        "/home/source/Program.cs",
                        """
                        using System;
                        class Program
                        {
                            static void Main() => Console.WriteLine(1);
                        }
                        """
                    ),
                },
            }
        };
        await test.RunAsync(cancellationToken);
    }
}
