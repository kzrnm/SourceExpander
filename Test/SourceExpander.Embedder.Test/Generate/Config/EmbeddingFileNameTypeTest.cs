using System.Collections.Immutable;

namespace SourceExpander.Generate.Config;

public class EmbeddingFileNameTypeTest : EmbedderGeneratorTestBase
{
    [Test]
    public async Task EmbeddingFileNameTypeWithoutCommonPrefix(CancellationToken cancellationToken)
    {
        const string embeddedSourceCode = "[{\"CodeBody\":\"class P { static void T() => Console.WriteLine(2); }\",\"Dependencies\":[],\"FileName\":\"TestProject>.cs\",\"TypeNames\":[\"P\"],\"Usings\":[\"using System;\"]},{\"CodeBody\":\"[DebuggerDisplay(\\\"Name\\\")] class Program { static void Main() { Console.WriteLine(1); }  [System.Diagnostics.Conditional(\\\"TEST\\\")] static void T() => Console.WriteLine(2); }\",\"Dependencies\":[],\"FileName\":\"TestProject>rogram.cs\",\"TypeNames\":[\"Program\"],\"Usings\":[\"using System;\",\"using System.Diagnostics;\"]}]";
        await embeddedSourceCode.Should().BeEquivalentToJsonSources([
             new SourceFileInfo
             (
                 "TestProject>.cs",
                 ["P"],
                 ImmutableArray.Create("using System;"),
                 ImmutableArray<string>.Empty,
                 """class P { static void T() => Console.WriteLine(2); }"""
             ),
             new SourceFileInfo
             (
                 "TestProject>rogram.cs",
                 ["Program"],
                 ImmutableArray.Create("using System;", "using System.Diagnostics;"),
                 ImmutableArray<string>.Empty,
                 """[DebuggerDisplay("Name")] class Program { static void Main() { Console.WriteLine(1); }  [System.Diagnostics.Conditional("TEST")] static void T() => Console.WriteLine(2); }"""
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
                        "embedding-filename-type": "withoutCommonPrefix"
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
                    (
                        "/home/source/P.cs",
                        """
                        using System;
                        class P
                        {
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
    public async Task EmbeddingFileNameTypeWithoutCommonPrefixProperty(CancellationToken cancellationToken)
    {
        const string embeddedSourceCode = "[{\"CodeBody\":\"class P { static void T() => Console.WriteLine(2); }\",\"Dependencies\":[],\"FileName\":\"TestProject>.cs\",\"TypeNames\":[\"P\"],\"Usings\":[\"using System;\"]},{\"CodeBody\":\"[DebuggerDisplay(\\\"Name\\\")] class Program { static void Main() { Console.WriteLine(1); }  [System.Diagnostics.Conditional(\\\"TEST\\\")] static void T() => Console.WriteLine(2); }\",\"Dependencies\":[],\"FileName\":\"TestProject>rogram.cs\",\"TypeNames\":[\"Program\"],\"Usings\":[\"using System;\",\"using System.Diagnostics;\"]}]";
        await embeddedSourceCode.Should().BeEquivalentToJsonSources([
             new SourceFileInfo
             (
                 "TestProject>.cs",
                 ["P"],
                 ImmutableArray.Create("using System;"),
                 ImmutableArray<string>.Empty,
                 """class P { static void T() => Console.WriteLine(2); }"""
             ),
             new SourceFileInfo
             (
                 "TestProject>rogram.cs",
                 ["Program"],
                 ImmutableArray.Create("using System;", "using System.Diagnostics;"),
                 ImmutableArray<string>.Empty,
                 """[DebuggerDisplay("Name")] class Program { static void Main() { Console.WriteLine(1); }  [System.Diagnostics.Conditional("TEST")] static void T() => Console.WriteLine(2); }"""
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
                { "build_property.SourceExpander_Embedder_EmbeddingFileNameType", "withoutCommonPrefix" },
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
                    (
                        "/home/source/P.cs",
                        """
                        using System;
                        class P
                        {
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
    public async Task EmbeddingFileNameTypeFullPath(CancellationToken cancellationToken)
    {
        const string embeddedSourceCode = "[{\"CodeBody\":\"class P { static void T() => Console.WriteLine(2); }\",\"Dependencies\":[],\"FileName\":\"/home/source/P.cs\",\"TypeNames\":[\"P\"],\"Usings\":[\"using System;\"]},{\"CodeBody\":\"[DebuggerDisplay(\\\"Name\\\")] class Program { static void Main() { Console.WriteLine(1); }  [System.Diagnostics.Conditional(\\\"TEST\\\")] static void T() => Console.WriteLine(2); }\",\"Dependencies\":[],\"FileName\":\"/home/source/Program.cs\",\"TypeNames\":[\"Program\"],\"Usings\":[\"using System;\",\"using System.Diagnostics;\"]}]";
        await embeddedSourceCode.Should().BeEquivalentToJsonSources([
             new SourceFileInfo
             (
                 "/home/source/P.cs",
                 ["P"],
                 ImmutableArray.Create("using System;"),
                 ImmutableArray<string>.Empty,
                 """class P { static void T() => Console.WriteLine(2); }"""
             ),
             new SourceFileInfo
             (
                 "/home/source/Program.cs",
                 ["Program"],
                 ImmutableArray.Create("using System;", "using System.Diagnostics;"),
                 ImmutableArray<string>.Empty,
                 """[DebuggerDisplay("Name")] class Program { static void Main() { Console.WriteLine(1); }  [System.Diagnostics.Conditional("TEST")] static void T() => Console.WriteLine(2); }"""
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
                        "embedding-filename-type": "fullpath"
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
                    (
                        "/home/source/P.cs",
                        """
                        using System;
                        class P
                        {
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
    public async Task EmbeddingFileNameTypeFullPathProperty(CancellationToken cancellationToken)
    {
        const string embeddedSourceCode = "[{\"CodeBody\":\"class P { static void T() => Console.WriteLine(2); }\",\"Dependencies\":[],\"FileName\":\"/home/source/P.cs\",\"TypeNames\":[\"P\"],\"Usings\":[\"using System;\"]},{\"CodeBody\":\"[DebuggerDisplay(\\\"Name\\\")] class Program { static void Main() { Console.WriteLine(1); }  [System.Diagnostics.Conditional(\\\"TEST\\\")] static void T() => Console.WriteLine(2); }\",\"Dependencies\":[],\"FileName\":\"/home/source/Program.cs\",\"TypeNames\":[\"Program\"],\"Usings\":[\"using System;\",\"using System.Diagnostics;\"]}]";
        await embeddedSourceCode.Should().BeEquivalentToJsonSources([
             new SourceFileInfo
             (
                 "/home/source/P.cs",
                 ["P"],
                 ImmutableArray.Create("using System;"),
                 ImmutableArray<string>.Empty,
                 """class P { static void T() => Console.WriteLine(2); }"""
             ),
             new SourceFileInfo
             (
                 "/home/source/Program.cs",
                 ["Program"],
                 ImmutableArray.Create("using System;", "using System.Diagnostics;"),
                 ImmutableArray<string>.Empty,
                 """[DebuggerDisplay("Name")] class Program { static void Main() { Console.WriteLine(1); }  [System.Diagnostics.Conditional("TEST")] static void T() => Console.WriteLine(2); }"""
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
                { "build_property.SourceExpander_Embedder_EmbeddingFileNameType", "fullpath" },
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
                    (
                        "/home/source/P.cs",
                        """
                        using System;
                        class P
                        {
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
