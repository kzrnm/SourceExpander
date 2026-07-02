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

    public static IEnumerable<string> EmbeddingGzipBase32768() => ["GzipBase32768", "Error"];

    [Test]
    [MethodDataSource(nameof(EmbeddingGzipBase32768))]
    public async Task EmbeddingGzipBase32768(string embeddingType, CancellationToken cancellationToken)
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
                    "/foo/bar/SourceExpander.Embedder.Config.json", $$"""
                    {
                        "$schema": "https://raw.githubusercontent.com/kzrnm/SourceExpander/master/schema/embedder.schema.json",
                        "embedding-type": "{{embeddingType}}",
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
    [MethodDataSource(nameof(EmbeddingGzipBase32768))]
    public async Task EmbeddingGzipBase32768Property(string embeddingType, CancellationToken cancellationToken)
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
                { "build_property.SourceExpander_Embedder_EmbeddingType", embeddingType },
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
    public async Task EmbeddingSingleMetadataJson(CancellationToken cancellationToken)
    {
        const string embeddedSourceCode = """[{"CodeBody":"[DebuggerDisplay(\"Name\")]class Program{static void Main(){Console.WriteLine(1);}[System.Diagnostics.Conditional(\"TEST\")]static void T()=>Console.WriteLine(2);}","Dependencies":[],"FileName":"TestProject>Program.cs","TypeNames":["Program"],"Usings":["using System;","using System.Diagnostics;"]}]""";
        ImmutableArray<SourceFileInfo> sources = [
             new SourceFileInfo
             (
                 "TestProject>Program.cs",
                 ["Program"],
                 ImmutableArray.Create("using System;", "using System.Diagnostics;"),
                 ImmutableArray<string>.Empty,
                 """[DebuggerDisplay("Name")]class Program{static void Main(){Console.WriteLine(1);}[System.Diagnostics.Conditional("TEST")]static void T()=>Console.WriteLine(2);}"""
             )
        ];
        await embeddedSourceCode.Should().BeEquivalentToJsonSources(sources);
        string entireJson =
            $$"""{"AssemblyName":"TestProject","Sources":{{embeddedSourceCode}},"EmbedderVersion":"{{EmbedderVersion}}","CSharpVersion":"{{EmbeddedLanguageVersion}}","AllowUnsafe":false,"EmbeddedNamespaces":[]}""";

        await System.Text.Json.JsonSerializer.Deserialize<EmbeddedData>(entireJson)
            .Should().BeEqualTo(new EmbeddedData(
                AssemblyName: "TestProject",
                AllowUnsafe: false,
                CSharpVersion: EmbeddedLanguageVersion,
                EmbedderVersion: Version.Parse(EmbedderVersion),
                EmbeddedNamespaces: [],
                Sources: sources), TestUtil.EmbeddedDataEqualityComparer);
        await Newtonsoft.Json.JsonConvert.DeserializeObject<EmbeddedData>(entireJson)
            .Should().BeEqualTo(new EmbeddedData(
                AssemblyName: "TestProject",
                AllowUnsafe: false,
                CSharpVersion: EmbeddedLanguageVersion,
                EmbedderVersion: Version.Parse(EmbedderVersion),
                EmbeddedNamespaces: [],
                Sources: sources), TestUtil.EmbeddedDataEqualityComparer);

        var test = new Test
        {
            TestState =
            {
                AdditionalFiles =
                {
                    (
                    "/foo/bar/SourceExpander.Embedder.Config.json", """
                    {
                        "$schema": "https://raw.githubusercontent.com/kzrnm/SourceExpander/master/schema/embedder.schema.json",
                        "embedding-type": "singlemetadatajson",
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
                GeneratedSources = {
                    (typeof(EmbedderGenerator), "EmbeddedSourceCode.Metadata.cs", EnvironmentUtil.JoinByStringBuilder([
                "// <auto-generated/>",
                "#pragma warning disable",
                $"[assembly: global::System.Reflection.AssemblyMetadataAttribute(\"SourceExpander.EmbeddedLanguageVersion\",{EmbedderVersion})]",
                $"[assembly: global::System.Reflection.AssemblyMetadataAttribute(\"SourceExpander.EmbeddedDataJson\",{entireJson.ToLiteral()})]"
                    ]))
                },
            }
        };
        await test.RunAsync(cancellationToken);
    }

    [Test]
    public async Task EmbeddingSingleMetadataJsonProperty(CancellationToken cancellationToken)
    {
        const string embeddedSourceCode = """[{"CodeBody":"[DebuggerDisplay(\"Name\")]class Program{static void Main(){Console.WriteLine(1);}[System.Diagnostics.Conditional(\"TEST\")]static void T()=>Console.WriteLine(2);}","Dependencies":[],"FileName":"TestProject>Program.cs","TypeNames":["Program"],"Usings":["using System;","using System.Diagnostics;"]}]""";
        ImmutableArray<SourceFileInfo> sources = [
             new SourceFileInfo
             (
                 "TestProject>Program.cs",
                 ["Program"],
                 ImmutableArray.Create("using System;", "using System.Diagnostics;"),
                 ImmutableArray<string>.Empty,
                 """[DebuggerDisplay("Name")]class Program{static void Main(){Console.WriteLine(1);}[System.Diagnostics.Conditional("TEST")]static void T()=>Console.WriteLine(2);}"""
             ),
        ];
        await embeddedSourceCode.Should().BeEquivalentToJsonSources(sources);
        string entireJson =
            $$"""{"AssemblyName":"TestProject","Sources":{{embeddedSourceCode}},"EmbedderVersion":"{{EmbedderVersion}}","CSharpVersion":"{{EmbeddedLanguageVersion}}","AllowUnsafe":false,"EmbeddedNamespaces":[]}""";

        await System.Text.Json.JsonSerializer.Deserialize<EmbeddedData>(entireJson)
            .Should().BeEqualTo(new EmbeddedData(
                AssemblyName: "TestProject",
                AllowUnsafe: false,
                CSharpVersion: EmbeddedLanguageVersion,
                EmbedderVersion: Version.Parse(EmbedderVersion),
                EmbeddedNamespaces: [],
                Sources: sources), TestUtil.EmbeddedDataEqualityComparer);
        await Newtonsoft.Json.JsonConvert.DeserializeObject<EmbeddedData>(entireJson)
            .Should().BeEqualTo(new EmbeddedData(
                AssemblyName: "TestProject",
                AllowUnsafe: false,
                CSharpVersion: EmbeddedLanguageVersion,
                EmbedderVersion: Version.Parse(EmbedderVersion),
                EmbeddedNamespaces: [],
                Sources: sources), TestUtil.EmbeddedDataEqualityComparer);

        var test = new Test
        {
            AnalyzerConfigOptions =
            {
                { "build_property.SourceExpander_Embedder_EmbeddingType", "SingleMetadataJson" },
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
                GeneratedSources = {
                    (typeof(EmbedderGenerator), "EmbeddedSourceCode.Metadata.cs", EnvironmentUtil.JoinByStringBuilder([
                "// <auto-generated/>",
                "#pragma warning disable",
                $"[assembly: global::System.Reflection.AssemblyMetadataAttribute(\"SourceExpander.EmbeddedLanguageVersion\",{EmbedderVersion})]",
                $"[assembly: global::System.Reflection.AssemblyMetadataAttribute(\"SourceExpander.EmbeddedDataJson\",{entireJson.ToLiteral()})]"
                    ]))
                },
            }
        };
        await test.RunAsync(cancellationToken);
    }
}
