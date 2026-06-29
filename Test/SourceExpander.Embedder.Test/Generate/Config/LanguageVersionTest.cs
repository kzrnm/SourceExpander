using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;

namespace SourceExpander.Generate.Config;

public class LanguageVersionTest : EmbedderGeneratorTestBase
{
    public static IEnumerable<(string inputLiteral, string embeddedVersion)> VersionCaces(bool isJson)
    {
        yield return LangVersionCases("4", "4");
        yield return LangVersionCases("7", "7.0");
        yield return LangVersionCases("7.3", "7.3");
        yield return LangVersionCases("10", "10.0");

        if (isJson)
        {
            yield return LangVersionCases("\"4\"", "4");
            yield return LangVersionCases("\"7\"", "7.0");
            yield return LangVersionCases("\"7.3\"", "7.3");
            yield return LangVersionCases("\"10\"", "10.0");
            yield return LangVersionCases("null", "preview");
        }

        static (string InputLiteral, string EmbeddedVersion) LangVersionCases([StringSyntax(StringSyntaxAttribute.Json)] string inputLiteral, string embeddedVersion)
            => (inputLiteral, embeddedVersion);
    }

    [Test]
    [MethodDataSource(nameof(VersionCaces), Arguments = [true])]
    public async Task Json(string inputLiteral, string embeddedLanguageVersion, CancellationToken cancellationToken)
    {
        const string embeddedSourceCode = "[{\"CodeBody\":\"class Program{static void Main(){Console.WriteLine(1);}}\",\"Dependencies\":[],\"FileName\":\"TestProject>Program.cs\",\"TypeNames\":[\"Program\"],\"Usings\":[\"using System;\"]}]";
        await embeddedSourceCode.Should().BeEquivalentToJsonSources([
             new SourceFileInfo
             (
                 "TestProject>Program.cs",
                 ["Program"],
                 // lang=C#
                 ImmutableArray.Create("using System;"),
                 ImmutableArray<string>.Empty,
                 // lang=C#
                 """class Program{static void Main(){Console.WriteLine(1);}}"""
             )
        ]);

        var test = new Test(new()
        {
            EmbeddedLanguageVersion = embeddedLanguageVersion,
            EmbeddedSourceCode = embeddedSourceCode,
        })
        {
            TestState =
            {
                AdditionalFiles =
                {
                    (
                    "/foo/bar/SourceExpander.Embedder.Config.json",
                    """
                    {
                        "language-version": "$langver$",
                        "embedding-type": "Raw",
                        "minify-level": "full"
                    }
                    """.Replace("\"$langver$\"", inputLiteral)),
                    ("/foo/bar/SourceExpander.Notmatch.json", "notmatch"),
                },
                Sources = {
                    (
                        "/home/source/Program.cs",
                        // lang=C#
                        """
                        using System;

                        class Program
                        {
                            static void Main()
                            {
                                Console.WriteLine(1);
                            }
                        }
                        """
                    ),
                },
            }
        };
        await test.RunAsync(cancellationToken);
    }

    [Test]
    [MethodDataSource(nameof(VersionCaces), Arguments = [false])]
    public async Task Property(string inputLiteral, string embeddedLanguageVersion, CancellationToken cancellationToken)
    {
        const string embeddedSourceCode = "[{\"CodeBody\":\"class Program{static void Main(){Console.WriteLine(1);}}\",\"Dependencies\":[],\"FileName\":\"TestProject>Program.cs\",\"TypeNames\":[\"Program\"],\"Usings\":[\"using System;\"]}]";
        await embeddedSourceCode.Should().BeEquivalentToJsonSources([
             new SourceFileInfo
             (
                 "TestProject>Program.cs",
                 ["Program"],
                 // lang=C#
                 ImmutableArray.Create("using System;"),
                 ImmutableArray<string>.Empty,
                 // lang=C#
                 """class Program{static void Main(){Console.WriteLine(1);}}"""
             )
        ]);

        var test = new Test(new()
        {
            EmbeddedLanguageVersion = embeddedLanguageVersion,
            EmbeddedSourceCode = embeddedSourceCode,
        })
        {
            AnalyzerConfigOptions =
            {
                { "build_property.SourceExpander_Embedder_LanguageVersion", inputLiteral },
            },
            TestState =
            {
                AdditionalFiles =
                {
                    (
                    "/foo/bar/SourceExpander.Embedder.Config.json",
                    """
                    {
                        "language-version": "1",
                        "embedding-type": "Raw",
                        "minify-level": "full"
                    }
                    """),
                    ("/foo/bar/SourceExpander.Notmatch.json", "notmatch"),
                },
                Sources = {
                    (
                        "/home/source/Program.cs",
                        // lang=C#
                        """
                        using System;

                        class Program
                        {
                            static void Main()
                            {
                                Console.WriteLine(1);
                            }
                        }
                        """
                    ),
                },
            }
        };
        await test.RunAsync(cancellationToken);
    }


    [Test]
    [Arguments(true)]
    [Arguments(false)]
    public async Task Error(bool hasError, CancellationToken cancellationToken)
    {
        const string embeddedSourceCode = "[{\"CodeBody\":\"class Program{static void M(int[]a){Console.WriteLine(a[..]);}}\",\"Dependencies\":[],\"FileName\":\"TestProject>Program.cs\",\"TypeNames\":[\"Program\"],\"Usings\":[\"using System;\"]}]";
        await embeddedSourceCode.Should().BeEquivalentToJsonSources([
             new SourceFileInfo
             (
                 "TestProject>Program.cs",
                 ["Program"],
                 // lang=C#
                 ImmutableArray.Create("using System;"),
                 ImmutableArray<string>.Empty,
                 // lang=C#
                 """class Program{static void M(int[]a){Console.WriteLine(a[..]);}}"""
             )
        ]);
        var langVersion = hasError ? "7.3" : "8.0";

        var test = new Test(new()
        {
            EmbeddedLanguageVersion = langVersion,
            EmbeddedSourceCode = embeddedSourceCode,
        })
        {
            AnalyzerConfigOptions =
            {
                { "build_property.SourceExpander_Embedder_LanguageVersion", langVersion },
            },
            TestState =
            {
                AdditionalFiles =
                {
                    (
                    "/foo/bar/SourceExpander.Embedder.Config.json",
                    """
                    {
                        "embedding-type": "Raw",
                        "minify-level": "full"
                    }
                    """),
                    ("/foo/bar/SourceExpander.Notmatch.json", "notmatch"),
                },
                Sources = {
                    (
                        "/home/source/Program.cs",
                        // lang=C#
                        """
                        using System;

                        class Program
                        {
                            static void M(int[] a)
                            {
                                Console.WriteLine(a[..]);
                            }
                        }
                        """
                    ),
                },
            }
        };

        if (hasError)
            test.TestState.ExpectedDiagnostics.Add(DiagnosticResult.CompilerWarning("EMBED0004"));

        await test.RunAsync(cancellationToken);
    }
}
