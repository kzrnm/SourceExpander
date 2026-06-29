using System.Collections.Immutable;

namespace SourceExpander.Generate.Config;

public class ExcludeAttributesTest : EmbedderGeneratorTestBase
{
    [Test]
    public async Task ExcludeAttributes(CancellationToken cancellationToken)
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
                    "/foo/bar/SourceExpander.Embedder.Config.json", """
                    {
                        "$schema": "https://raw.githubusercontent.com/kzrnm/SourceExpander/master/schema/embedder.schema.json",
                        "embedding-type": "Raw",
                        "exclude-attributes": [
                            "System.Diagnostics.DebuggerDisplayAttribute"
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
    public async Task ExcludeAttributesProperty(CancellationToken cancellationToken)
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
                { "build_property.SourceExpander_Embedder_ExcludeAttributes", "System.Diagnostics.DebuggerDisplayAttribute;System.Diagnostics.DebuggerDisplayAttribute" },
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
