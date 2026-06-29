using System.Collections.Immutable;

namespace SourceExpander.Generate.Config;

public class IncludeExcludeTest : EmbedderGeneratorTestBase
{
    public static IEnumerable<Func<string[]>> Include_Data()
    {
        yield return () => ["/home/**"];
        yield return () => ["/home/**/*.cs"];
        yield return () => ["/home/source/Program.cs"];
    }

    [Test]
    [MethodDataSource(nameof(Include_Data))]
    public async Task Include(string[] data, CancellationToken cancellationToken)
    {
        const string embeddedSourceCode = """[{"CodeBody":"class Program{static void Main(){Console.WriteLine(1);}[System.Diagnostics.Conditional(\"TEST\")]static void T()=>Console.WriteLine(2);}","Dependencies":[],"FileName":"TestProject>Program.cs","TypeNames":["Program"],"Usings":["using System;"]}]""";
        await embeddedSourceCode.Should().BeEquivalentToJsonSources([
             new SourceFileInfo
             (
                 "TestProject>Program.cs",
                 ["Program"],
                 ImmutableArray.Create("using System;"),
                 ImmutableArray<string>.Empty,
                 """class Program{static void Main(){Console.WriteLine(1);}[System.Diagnostics.Conditional("TEST")]static void T()=>Console.WriteLine(2);}"""
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
                    $$$"""
                    {
                        "$schema": "https://raw.githubusercontent.com/kzrnm/SourceExpander/master/schema/embedder.schema.json",
                        "embedding-type": "Raw",
                        "include": [{{{string.Join(",", data.Select(RoslynUtil.ToLiteral))}}}],
                        "minify-level": "full"
                    }
                    """),
                    ("/foo/bar/SourceExpander.Notmatch.json", "notmatch"),
                },
                Sources = {
                    (
                        "/other/Dummy.cs",
                        """class Dummy{}"""
                    ),
                    (
                        "/other/Dummy2.cs",
                        """class Dummy2{}"""
                    ),
                    (
                        "/home/source/Program.cs",
                        """
                        using System;
                        using System.Diagnostics;

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
    public async Task IncludeProperty(CancellationToken cancellationToken)
    {
        const string embeddedSourceCode = """[{"CodeBody":"class Program{static void Main(){Console.WriteLine(1);}[System.Diagnostics.Conditional(\"TEST\")]static void T()=>Console.WriteLine(2);}","Dependencies":[],"FileName":"TestProject>Program.cs","TypeNames":["Program"],"Usings":["using System;"]}]""";
        await embeddedSourceCode.Should().BeEquivalentToJsonSources([
             new SourceFileInfo
             (
                 "TestProject>Program.cs",
                 ["Program"],
                 ImmutableArray.Create("using System;"),
                 ImmutableArray<string>.Empty,
                 """class Program{static void Main(){Console.WriteLine(1);}[System.Diagnostics.Conditional("TEST")]static void T()=>Console.WriteLine(2);}"""
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
                { "build_property.SourceExpander_Embedder_Include", "/home/**" },
            },
            TestState =
            {
                Sources = {
                    (
                        "/other/Dummy.cs",
                        """class Dummy{}"""
                    ),
                    (
                        "/other/Dummy2.cs",
                        """class Dummy2{}"""
                    ),
                    (
                        "/home/source/Program.cs",
                        """
using System;
using System.Diagnostics;

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

    public static IEnumerable<Func<string[]>> Exclude_Data()
    {
        yield return () => ["/other/**"];
        yield return () => ["/other/**/*.cs"];
        yield return () => ["/other/Dummy.cs", "/other/Dummy2.cs"];
    }

    [Test]
    [MethodDataSource(nameof(Exclude_Data))]
    public async Task Exclude(string[] data, CancellationToken cancellationToken)
    {
        const string embeddedSourceCode = """[{"CodeBody":"class Program{static void Main(){Console.WriteLine(1);}[System.Diagnostics.Conditional(\"TEST\")]static void T()=>Console.WriteLine(2);}","Dependencies":[],"FileName":"TestProject>Program.cs","TypeNames":["Program"],"Usings":["using System;"]}]""";
        await embeddedSourceCode.Should().BeEquivalentToJsonSources([
             new SourceFileInfo
             (
                 "TestProject>Program.cs",
                 ["Program"],
                 ImmutableArray.Create("using System;"),
                 ImmutableArray<string>.Empty,
                 """class Program{static void Main(){Console.WriteLine(1);}[System.Diagnostics.Conditional("TEST")]static void T()=>Console.WriteLine(2);}"""
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
                    $$$"""
                    {
                        "$schema": "https://raw.githubusercontent.com/kzrnm/SourceExpander/master/schema/embedder.schema.json",
                        "embedding-type": "Raw",
                        "exclude": [{{{string.Join(",", data.Select(RoslynUtil.ToLiteral))}}}],
                        "minify-level": "full"
                    }
                    """),
                    ("/foo/bar/SourceExpander.Notmatch.json", "notmatch"),
                },
                Sources = {
                    (
                        "/other/Dummy.cs",
                        """class Dummy{}"""
                    ),
                    (
                        "/other/Dummy2.cs",
                        """class Dummy2{}"""
                    ),
                    (
                        "/home/source/Program.cs",
                        """
                        using System;
                        using System.Diagnostics;

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
    public async Task ExcludeProperty(CancellationToken cancellationToken)
    {
        const string embeddedSourceCode = """[{"CodeBody":"class Program{static void Main(){Console.WriteLine(1);}[System.Diagnostics.Conditional(\"TEST\")]static void T()=>Console.WriteLine(2);}","Dependencies":[],"FileName":"TestProject>Program.cs","TypeNames":["Program"],"Usings":["using System;"]}]""";
        await embeddedSourceCode.Should().BeEquivalentToJsonSources([
             new SourceFileInfo
             (
                 "TestProject>Program.cs",
                 ["Program"],
                 ImmutableArray.Create("using System;"),
                 ImmutableArray<string>.Empty,
                 """class Program{static void Main(){Console.WriteLine(1);}[System.Diagnostics.Conditional("TEST")]static void T()=>Console.WriteLine(2);}"""
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
                { "build_property.SourceExpander_Embedder_Exclude", "/other/**" }
            },
            TestState =
            {
                Sources = {
                    (
                        "/other/Dummy.cs",
                        """class Dummy{}"""
                    ),
                    (
                        "/other/Dummy2.cs",
                        """class Dummy2{}"""
                    ),
                    (
                        "/home/source/Program.cs",
                        """
using System;
using System.Diagnostics;

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
