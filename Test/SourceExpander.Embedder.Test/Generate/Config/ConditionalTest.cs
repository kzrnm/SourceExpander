using System.Collections.Immutable;

namespace SourceExpander.Generate.Config;

public class ConditionalTest : EmbedderGeneratorTestBase
{
    [Test]
    public async Task Conditional(CancellationToken cancellationToken)
    {
        const string embeddedSourceCode = """[{"CodeBody":"class Program{static void Main(){T();Console.WriteLine(1);}[System.Diagnostics.Conditional(\"TEST\")]static void T()=>Console.WriteLine(2);[Conditional(\"DEBUG2\")]static void T4()=>Console.WriteLine(4);[System.Diagnostics.Conditional(\"DEBUG2\")][Conditional(\"Test\")]static void T8()=>Console.WriteLine(8);}","Dependencies":[],"FileName":"TestProject>Program.cs","TypeNames":["Program"],"Usings":["using System;","using System.Diagnostics;"]}]""";
        await embeddedSourceCode.Should().BeEquivalentToJsonSources([
             new SourceFileInfo
             (
                 "TestProject>Program.cs",
                 ["Program"],
                 ImmutableArray.Create("using System;", "using System.Diagnostics;"),
                 ImmutableArray<string>.Empty,
                 """class Program{static void Main(){T();Console.WriteLine(1);}[System.Diagnostics.Conditional("TEST")]static void T()=>Console.WriteLine(2);[Conditional("DEBUG2")]static void T4()=>Console.WriteLine(4);[System.Diagnostics.Conditional("DEBUG2")][Conditional("Test")]static void T8()=>Console.WriteLine(8);}"""
             )
        ]);

        var test = new Test(new()
        {
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
                            "$schema": "https://raw.githubusercontent.com/kzrnm/SourceExpander/master/schema/embedder.schema.json",
                            "embedding-type": "Raw",
                            "remove-conditional": [
                                "DEBUG", "DEBUG2"
                            ],
                            "minify-level": "full"
                        }
                        """),
                    ("/foo/bar/SourceExpander.Notmatch.json", "notmatch"),
                },
                Sources = {
                    (
                        "/home/source/Program.cs",
                        """
                        using System;
                        using System.Diagnostics;

                        class Program
                        {
                            static void Main()
                            {
                                Debug.Assert(true);
                                T();
                                T4();
                                T8();
                                Console.WriteLine(1);
                            }

                            [System.Diagnostics.Conditional("TEST")]
                            static void T() => Console.WriteLine(2);
                            [Conditional("DEBUG2")]
                            static void T4() => Console.WriteLine(4);
                            [System.Diagnostics.Conditional("DEBUG2")]
                            [Conditional("Test")]
                            static void T8() => Console.WriteLine(8);
                        }
                        """
                    ),
                },
            }
        };
        await test.RunAsync(cancellationToken);
    }

    [Test]
    public async Task ConditionalProperty(CancellationToken cancellationToken)
    {
        const string embeddedSourceCode = """[{"CodeBody":"class Program{static void Main(){T();Console.WriteLine(1);}[System.Diagnostics.Conditional(\"TEST\")]static void T()=>Console.WriteLine(2);[Conditional(\"DEBUG2\")]static void T4()=>Console.WriteLine(4);[System.Diagnostics.Conditional(\"DEBUG2\")][Conditional(\"Test\")]static void T8()=>Console.WriteLine(8);}","Dependencies":[],"FileName":"TestProject>Program.cs","TypeNames":["Program"],"Usings":["using System;","using System.Diagnostics;"]}]""";
        await embeddedSourceCode.Should().BeEquivalentToJsonSources([
             new SourceFileInfo
             (
                 "TestProject>Program.cs",
                 ["Program"],
                 ImmutableArray.Create("using System;", "using System.Diagnostics;"),
                 ImmutableArray<string>.Empty,
                 """class Program{static void Main(){T();Console.WriteLine(1);}[System.Diagnostics.Conditional("TEST")]static void T()=>Console.WriteLine(2);[Conditional("DEBUG2")]static void T4()=>Console.WriteLine(4);[System.Diagnostics.Conditional("DEBUG2")][Conditional("Test")]static void T8()=>Console.WriteLine(8);}"""
             )
        ]);

        var test = new Test(new()
        {
            EmbeddedSourceCode = embeddedSourceCode,
        })
        {
            AnalyzerConfigOptions =
            {
                { "build_property.SourceExpander_Embedder_EmbeddingType", "Raw" },
                { "build_property.SourceExpander_Embedder_MinifyLevel", "full" },
                { "build_property.SourceExpander_Embedder_RemoveConditional", "DEBUG;DEBUG2" },
            },
            TestState =
            {
                Sources = {
                    (
                        "/home/source/Program.cs",
                        """
                        using System;
                        using System.Diagnostics;

                        class Program
                        {
                            static void Main()
                            {
                                Debug.Assert(true);
                                T();
                                T4();
                                T8();
                                Console.WriteLine(1);
                            }

                            [System.Diagnostics.Conditional("TEST")]
                            static void T() => Console.WriteLine(2);
                            [Conditional("DEBUG2")]
                            static void T4() => Console.WriteLine(4);
                            [System.Diagnostics.Conditional("DEBUG2")]
                            [Conditional("Test")]
                            static void T8() => Console.WriteLine(8);
                        }
                        """
                    ),
                },
            }
        };
        await test.RunAsync(cancellationToken);
    }

    [Test]
    public async Task ConditionalNone(CancellationToken cancellationToken)
    {
        const string embeddedSourceCode = """[{"CodeBody":"class Program{static void Main(){Debug.Assert(true);Console.WriteLine(1);}}","Dependencies":[],"FileName":"TestProject>Program.cs","TypeNames":["Program"],"Usings":["using System;","using System.Diagnostics;"]}]""";
        await embeddedSourceCode.Should().BeEquivalentToJsonSources([
             new SourceFileInfo
             (
                 "TestProject>Program.cs",
                 ["Program"],
                 ImmutableArray.Create("using System;", "using System.Diagnostics;"),
                 ImmutableArray<string>.Empty,
                 """class Program{static void Main(){Debug.Assert(true);Console.WriteLine(1);}}"""
             )
        ]);

        var test = new Test(new()
        {
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
                        "$schema": "https://raw.githubusercontent.com/kzrnm/SourceExpander/master/schema/embedder.schema.json",
                        "embedding-type": "Raw",
                        "minify-level": "full"
                    }
                    """),
                    ("/foo/bar/SourceExpander.Notmatch.json", "notmatch"),
                },
                Sources = {
                    (
                        "/home/source/Program.cs",
                        """
                        using System;
                        using System.Diagnostics;

                        class Program
                        {
                            static void Main()
                            {
                                Debug.Assert(true);
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
    public async Task ConditionalNoneProperty(CancellationToken cancellationToken)
    {
        const string embeddedSourceCode = """[{"CodeBody":"class Program{static void Main(){Debug.Assert(true);Console.WriteLine(1);}}","Dependencies":[],"FileName":"TestProject>Program.cs","TypeNames":["Program"],"Usings":["using System;","using System.Diagnostics;"]}]""";
        await embeddedSourceCode.Should().BeEquivalentToJsonSources([
             new SourceFileInfo
             (
                 "TestProject>Program.cs",
                 ["Program"],
                 ImmutableArray.Create("using System;", "using System.Diagnostics;"),
                 ImmutableArray<string>.Empty,
                 """class Program{static void Main(){Debug.Assert(true);Console.WriteLine(1);}}"""
             )
        ]);

        var test = new Test(new()
        {
            EmbeddedSourceCode = embeddedSourceCode,
        })
        {
            AnalyzerConfigOptions =
            {
                { "build_property.SourceExpander_Embedder_EmbeddingType", "raw" },
                { "build_property.SourceExpander_Embedder_MinifyLevel", "full" },
            },
            TestState =
            {
                Sources = {
                    (
                        "/home/source/Program.cs",
                        """
                        using System;
                        using System.Diagnostics;

                        class Program
                        {
                            static void Main()
                            {
                                Debug.Assert(true);
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
