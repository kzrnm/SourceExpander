using System.Collections.Immutable;

namespace SourceExpander.Generate;

public class DefaultTest : EmbedderGeneratorTestBase
{
    [Test]
    public async Task MinifyRaw(CancellationToken cancellationToken)
    {
        const string embeddedSourceCode = """[{"CodeBody":"class Program{static void Main()=>Console.WriteLine(0);}","Dependencies":[],"FileName":"TestProject>Program.cs","TypeNames":["Program"],"Usings":["using System;"]}]""";
        await embeddedSourceCode.Should().BeEquivalentToJsonSources([
             new SourceFileInfo
             (
                 "TestProject>Program.cs",
                 ["Program"],
                 ImmutableArray.Create("using System;"),
                 ImmutableArray<string>.Empty,
                 """class Program{static void Main()=>Console.WriteLine(0);}"""
             ),
        ]);

        var test = new Test(new()
        {
            EmbeddedSourceCode = embeddedSourceCode,
        })
        {
            TestState =
            {
                AdditionalFiles = { enableMinifyJson },
                Sources = {
                    (
                        "/home/source/Program.cs",
                        """
                        using System;
                        class Program
                        {
                            static void Main() => Console.WriteLine(0);
                        }
                        """
                    ),
                },
            }
        };
        await test.RunAsync(cancellationToken);
    }

    [Test]
    public async Task Generate(CancellationToken cancellationToken)
    {
        const string embeddedSourceCode = """[{"CodeBody":"class Program { static void Main() => Console.WriteLine(0); }","Dependencies":[],"FileName":"TestProject>Program.cs","TypeNames":["Program"],"Usings":["using System;"]}]""";
        await embeddedSourceCode.Should().BeEquivalentToJsonSources([
             new SourceFileInfo
             (
                 "TestProject>Program.cs",
                 ["Program"],
                 ImmutableArray.Create("using System;"),
                 ImmutableArray<string>.Empty,
                 """class Program { static void Main() => Console.WriteLine(0); }"""
             ),
        ]);

        var test = new Test(new()
        {
            EmbeddedSourceCodeGZip = embeddedSourceCode,
        })
        {
            TestState =
            {
                AdditionalFiles = { },
                Sources = {
                    (
                        "/home/source/Program.cs",
                        """
                        using System;
                        class Program
                        {
                            static void Main() => Console.WriteLine(0);
                        }

                        """
                    ),
                },
            }
        };
        await test.RunAsync(cancellationToken);
    }
}
