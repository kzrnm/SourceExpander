using System.Collections.Immutable;

namespace SourceExpander.Generate.Config;

public class EmbeddingTypeTest : EmbedderGeneratorTestBase
{
    [Test]
    public async Task EmbeddingRaw(CancellationToken cancellationToken)
    {
        const string embeddedSourceCode = """[{"CodeBody":"[DebuggerDisplay(\"Name\")] class Program { static void Main() { Console.WriteLine(1); }  [System.Diagnostics.Conditional(\"TEST\")] static void T() => Console.WriteLine(2); }","Dependencies":[],"FileName":"TestProject>Program.cs","TypeNames":["Program"],"Usings":["using System;","using System.Diagnostics;"]}]""";
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
            EmbeddedSourceCode = embeddedSourceCode,
        })
        {
            TestState =
            {
                AdditionalFiles =
                {
                    (
                    "/foo/bar/SourceExpander.Embedder.Config.json", """
                    {
                        "$schema": "https://raw.githubusercontent.com/kzrnm/SourceExpander/master/schema/embedder.schema.json",
                        "embedding-type": "Raw"
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
    public async Task EmbeddingRawProperty(CancellationToken cancellationToken)
    {
        const string embeddedSourceCode = """[{"CodeBody":"[DebuggerDisplay(\"Name\")] class Program { static void Main() { Console.WriteLine(1); }  [System.Diagnostics.Conditional(\"TEST\")] static void T() => Console.WriteLine(2); }","Dependencies":[],"FileName":"TestProject>Program.cs","TypeNames":["Program"],"Usings":["using System;","using System.Diagnostics;"]}]""";
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
            EmbeddedSourceCode = embeddedSourceCode,
        })
        {
            AnalyzerConfigOptions =
            {
                { "build_property.SourceExpander_Embedder_EmbeddingType", "raw" },
            },
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
    public async Task EmbeddingRawNoMinify(CancellationToken cancellationToken)
    {
        const string embeddedSourceCode = """[{"CodeBody":"[DebuggerDisplay(\"Name\")]\nclass Program\n{\n    static void Main()\n    {\n        Console.WriteLine(1);\n    }\n\n    [System.Diagnostics.Conditional(\"TEST\")]\n    static void T() => Console.WriteLine(2);\n}","Dependencies":[],"FileName":"TestProject>Program.cs","TypeNames":["Program"],"Usings":["using System;","using System.Diagnostics;"]}]""";
        await embeddedSourceCode.Should().BeEquivalentToJsonSources([
             new SourceFileInfo
             (
                 "TestProject>Program.cs",
                 ["Program"],
                 ImmutableArray.Create("using System;", "using System.Diagnostics;"),
                 ImmutableArray<string>.Empty,
                 """
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
                 """.Replace("\r\n", "\n")
             ),
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
                        "minify-level": "off"
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
    public async Task EmbeddingRawNoMinifyProperty(CancellationToken cancellationToken)
    {
        const string embeddedSourceCode = """[{"CodeBody":"[DebuggerDisplay(\"Name\")]\nclass Program\n{\n    static void Main()\n    {\n        Console.WriteLine(1);\n    }\n\n    [System.Diagnostics.Conditional(\"TEST\")]\n    static void T() => Console.WriteLine(2);\n}","Dependencies":[],"FileName":"TestProject>Program.cs","TypeNames":["Program"],"Usings":["using System;","using System.Diagnostics;"]}]""";
        await embeddedSourceCode.Should().BeEquivalentToJsonSources([
             new SourceFileInfo
             (
                 "TestProject>Program.cs",
                 ["Program"],
                 ImmutableArray.Create("using System;", "using System.Diagnostics;"),
                 ImmutableArray<string>.Empty,
                 """
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
                 """.Replace("\r\n", "\n")
             ),
        ]);

        var test = new Test(new()
        {
            EmbeddedSourceCode = embeddedSourceCode,
        })
        {
            AnalyzerConfigOptions =
            {
                { "build_property.SourceExpander_Embedder_EmbeddingType", "raw" },
                { "build_property.SourceExpander_Embedder_MinifyLevel", "off" },
            },
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
    public async Task EmbeddingRawFullMinify(CancellationToken cancellationToken)
    {
        const string embeddedSourceCode = """[{"CodeBody":"[DebuggerDisplay(\"Name\")]class Program{static void Main(){Console.WriteLine(1);}[System.Diagnostics.Conditional(\"TEST\")]static void T()=>Console.WriteLine(2);}","Dependencies":[],"FileName":"TestProject>Program.cs","TypeNames":["Program"],"Usings":["using System;","using System.Diagnostics;"]}]""";
        await embeddedSourceCode.Should().BeEquivalentToJsonSources([
             new SourceFileInfo
             (
                 "TestProject>Program.cs",
                 ["Program"],
                 ImmutableArray.Create("using System;", "using System.Diagnostics;"),
                 ImmutableArray<string>.Empty,
                 """[DebuggerDisplay("Name")]class Program{static void Main(){Console.WriteLine(1);}[System.Diagnostics.Conditional("TEST")]static void T()=>Console.WriteLine(2);}"""
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
                    "/foo/bar/SourceExpander.Embedder.Config.json", """
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
    public async Task EmbeddingRawFullMinifyProperty(CancellationToken cancellationToken)
    {
        const string embeddedSourceCode = """[{"CodeBody":"[DebuggerDisplay(\"Name\")]class Program{static void Main(){Console.WriteLine(1);}[System.Diagnostics.Conditional(\"TEST\")]static void T()=>Console.WriteLine(2);}","Dependencies":[],"FileName":"TestProject>Program.cs","TypeNames":["Program"],"Usings":["using System;","using System.Diagnostics;"]}]""";
        await embeddedSourceCode.Should().BeEquivalentToJsonSources([
             new SourceFileInfo
             (
                 "TestProject>Program.cs",
                 ["Program"],
                 ImmutableArray.Create("using System;", "using System.Diagnostics;"),
                 ImmutableArray<string>.Empty,
                 """[DebuggerDisplay("Name")]class Program{static void Main(){Console.WriteLine(1);}[System.Diagnostics.Conditional("TEST")]static void T()=>Console.WriteLine(2);}"""
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
    public async Task EmbeddingGzipBase32768(CancellationToken cancellationToken)
    {
        const string embeddedSourceCode = """[{"CodeBody":"[DebuggerDisplay(\"Name\")]class Program{static void Main(){Console.WriteLine(1);}[System.Diagnostics.Conditional(\"TEST\")]static void T()=>Console.WriteLine(2);}","Dependencies":[],"FileName":"TestProject>Program.cs","TypeNames":["Program"],"Usings":["using System;","using System.Diagnostics;"]}]""";
        await embeddedSourceCode.Should().BeEquivalentToJsonSources([
             new SourceFileInfo
             (
                 "TestProject>Program.cs",
                 ["Program"],
                 ImmutableArray.Create("using System;", "using System.Diagnostics;"),
                 ImmutableArray<string>.Empty,
                 """[DebuggerDisplay("Name")]class Program{static void Main(){Console.WriteLine(1);}[System.Diagnostics.Conditional("TEST")]static void T()=>Console.WriteLine(2);}"""
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
                    (
                    "/foo/bar/SourceExpander.Embedder.Config.json", """
                    {
                        "$schema": "https://raw.githubusercontent.com/kzrnm/SourceExpander/master/schema/embedder.schema.json",
                        "embedding-type": "gzip-base32768",
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
    public async Task EmbeddingGzipBase32768Property(CancellationToken cancellationToken)
    {
        const string embeddedSourceCode = """[{"CodeBody":"[DebuggerDisplay(\"Name\")]class Program{static void Main(){Console.WriteLine(1);}[System.Diagnostics.Conditional(\"TEST\")]static void T()=>Console.WriteLine(2);}","Dependencies":[],"FileName":"TestProject>Program.cs","TypeNames":["Program"],"Usings":["using System;","using System.Diagnostics;"]}]""";
        await embeddedSourceCode.Should().BeEquivalentToJsonSources([
             new SourceFileInfo
             (
                 "TestProject>Program.cs",
                 ["Program"],
                 ImmutableArray.Create("using System;", "using System.Diagnostics;"),
                 ImmutableArray<string>.Empty,
                 """[DebuggerDisplay("Name")]class Program{static void Main(){Console.WriteLine(1);}[System.Diagnostics.Conditional("TEST")]static void T()=>Console.WriteLine(2);}"""
             )
        ]);

        var test = new Test(new()
        {
            EmbeddedSourceCodeGZip = embeddedSourceCode,
        })
        {
            AnalyzerConfigOptions =
            {
                { "build_property.SourceExpander_Embedder_EmbeddingType", "gzip-base32768" },
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
}
