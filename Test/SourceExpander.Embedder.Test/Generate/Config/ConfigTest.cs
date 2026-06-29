using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Testing;

namespace SourceExpander.Generate.Config;

public class ConfigTest : EmbedderGeneratorTestBase
{
    public static IEnumerable<Func<(InMemorySourceText, object[])>> ParseErrorJsons()
    {
        yield return () => (
            new(
            "/foo/directory/SourceExpander.Embedder.Config.json", """
            {
                "$schema": "https://raw.githubusercontent.com/kzrnm/SourceExpander/master/schema/embedder.schema.json",
                "embedding-type": "Raw",
                "exclude-attributes": 1
            }
            """),
            new object[]
            {
                "/foo/directory/SourceExpander.Embedder.Config.json",
                "Error converting value 1 to type 'System.String[]'. Path 'exclude-attributes', line 4, position 27."
            }
        );
        yield return () => (
            new(
            "/foo/bar/sourceExpander.embedder.config.json", """
            {
                "$schema": "https://raw.githubusercontent.com/kzrnm/SourceExpander/master/schema/embedder.schema.json",
                "embedding-type": "Raw",
                "exclude-attributes": 1
            }
            """),
            new object[]
            {
                "/foo/bar/sourceExpander.embedder.config.json",
                "Error converting value 1 to type 'System.String[]'. Path 'exclude-attributes', line 4, position 27."
            }
        );
    }

    [Test]
    [MethodDataSource(nameof(ParseErrorJsons))]
    public async Task ParseError(InMemorySourceText additionalText, object[] diagnosticsArg, CancellationToken cancellationToken)
    {
        string embeddedSourceCode = """[{"CodeBody":"[DebuggerDisplay(\"Name\")] class Program { static void Main() { Console.WriteLine(1); }  [System.Diagnostics.Conditional(\"TEST\")] static void T() => Console.WriteLine(2); }","Dependencies":[],"FileName":"TestProject>Program.cs","TypeNames":["Program"],"Usings":["using System;","using System.Diagnostics;"]}]""";
        await embeddedSourceCode.Should().BeEquivalentToJsonSources([
             new SourceFileInfo
             (
                 "TestProject>Program.cs",
                 ["Program"],
                 ImmutableArray.Create("using System;", "using System.Diagnostics;"),
                 ImmutableArray<string>.Empty,
                 """[DebuggerDisplay("Name")] class Program { static void Main() { Console.WriteLine(1); }  [System.Diagnostics.Conditional("TEST")] static void T() => Console.WriteLine(2); }"""
             )
        ]);
        var test = new Test(new()
        {
            EmbeddedSourceCodeGZip = embeddedSourceCode,
        })
        {
            TestState =
            {
                AdditionalFiles =
                {
                    additionalText,
                    ("/foo/bar/SourceExpander.Notmatch.json", "notmatch"),
                },
                Sources = {
                    (
                        "/home/source/Program.cs",
                        """
                        using System;
                        using System.Diagnostics;

                        [DebuggerDisplay("Name")]
                        class Program
                        {
                            static void Main()
                            {
                                Console.WriteLine(1);
                            }

                            [System.Diagnostics.Conditional("TEST")]
                            static void T() => Console.WriteLine(2);
                        }
                        """
                    ),
                },
                ExpectedDiagnostics =
                {
                    DiagnosticResult.CompilerError("EMBED0003")
                        .WithSpan(additionalText.Path, 1, 1, 1, 1)
                        .WithArguments(diagnosticsArg),
                }
            }
        };
        await test.RunAsync(cancellationToken);
    }

    [Test]
    public async Task WithoutConfig(CancellationToken cancellationToken)
    {
        string embeddedSourceCode = """[{"CodeBody":"[DebuggerDisplay(\"Name\")] class Program { static void Main() { Console.WriteLine(1); }  [System.Diagnostics.Conditional(\"TEST\")] static void T() => Console.WriteLine(2); }","Dependencies":[],"FileName":"TestProject>Program.cs","TypeNames":["Program"],"Usings":["using System;","using System.Diagnostics;"]}]""";
        await embeddedSourceCode.Should().BeEquivalentToJsonSources([
             new SourceFileInfo
             (
                 "TestProject>Program.cs",
                 ["Program"],
                 ImmutableArray.Create("using System;", "using System.Diagnostics;"),
                 ImmutableArray<string>.Empty,
                 """[DebuggerDisplay("Name")] class Program { static void Main() { Console.WriteLine(1); }  [System.Diagnostics.Conditional("TEST")] static void T() => Console.WriteLine(2); }"""
             )
        ]);

        var test = new Test(new()
        {
            EmbeddedSourceCodeGZip = embeddedSourceCode,
        })
        {
            TestState =
            {
                Sources = {
                    (
                        "/home/source/Program.cs",
                        """
                        using System;
                        using System.Diagnostics;

                        [DebuggerDisplay("Name")]
                        class Program
                        {
                            static void Main()
                            {
                                Console.WriteLine(1);
                            }

                            [System.Diagnostics.Conditional("TEST")]
                            static void T() => Console.WriteLine(2);
                        }
                        """
                    ),
                },
            }
        };
        await test.RunAsync(cancellationToken);
    }

    [Test]
    public async Task NotEnabled(CancellationToken cancellationToken)
    {
        var test = new Test
        {
            TestState =
            {
                AdditionalFiles =
                {
                    (
                    "/foo/bar/SourceExpander.Embedder.Config.json",
                    """
                    {
                        "$schema": "https://raw.githubusercontent.com/kzrnm/SourceExpander/master/schema/embedder.schema.json",
                        "embedding-type": "Raw",
                        "enabled": false,
                        "exclude-attributes": [
                            "System.Diagnostics.DebuggerDisplayAttribute"
                        ]
                    }
                    """),
                    ("/foo/bar/SourceExpander.Notmatch.json", "notmatch"),
                },
                Sources = {
                    (
                        "/home/source/Program.cs",
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
    public async Task NotEnabledProperty(CancellationToken cancellationToken)
    {
        var test = new Test
        {
            AnalyzerConfigOptions =
            {
                { "build_property.SourceExpander_Embedder_Enabled", "false" },
            },
            TestState =
            {
                Sources = {
                    (
                        "/home/source/Program.cs",
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
}
