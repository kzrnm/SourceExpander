using System.Collections.Immutable;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace SourceExpander.Generate
{
    public class NullableDirectiveTest : EmbedderGeneratorTestBase
    {
        [Fact]
        public async Task Generate()
        {
            var embeddedFiles = ImmutableArray.Create(
                 new SourceFileInfo
                 (
                     "TestProject>Program.cs",
                     new string[] { "Program" },
                     ImmutableArray.Create("using System;"),
                     ImmutableArray.Create<string>(),
                     @"class Program{static void Main()=>Console.WriteLine(0);}"
                 ));

            const string embeddedSourceCode = "[{\"CodeBody\":\"class Program{static void Main()=>Console.WriteLine(0);}\",\"Dependencies\":[],\"FileName\":\"TestProject>Program.cs\",\"TypeNames\":[\"Program\"],\"Usings\":[\"using System;\"]}]";

            var test = new Test
            {
                TestState =
                {
                    AdditionalFiles = { enableMinifyJson },
                    Sources = {
                        (
                            "/home/source/Program.cs",
                            @"using System;
#nullable enable
class Program
{
    static void Main() => Console.WriteLine(0);
}
"
                        ),
                    },
                    ExpectedDiagnostics =
                    {
                        DiagnosticResult.CompilerWarning("EMBED0008").WithSpan("/home/source/Program.cs", 2, 1, 2, 17),
                    },
                    GeneratedSources =
                    {
                        (typeof(EmbedderGenerator), "EmbeddedSourceCode.Metadata.cs",
                        EnvironmentUtil.JoinByStringBuilder("using System.Reflection;",
                        $"[assembly: AssemblyMetadataAttribute(\"SourceExpander.EmbedderVersion\",\"{EmbedderVersion}\")]",
                        $"[assembly: AssemblyMetadataAttribute(\"SourceExpander.EmbeddedLanguageVersion\",\"{EmbeddedLanguageVersion}\")]",
                        $"[assembly: AssemblyMetadataAttribute(\"SourceExpander.EmbeddedSourceCode\",{embeddedSourceCode.ToLiteral()})]")
                        ),
                    }
                }
            };
            await test.RunAsync();
            Newtonsoft.Json.JsonConvert.DeserializeObject<SourceFileInfo[]>(embeddedSourceCode)
                .Should()
                .BeEquivalentTo(embeddedFiles);
            System.Text.Json.JsonSerializer.Deserialize<SourceFileInfo[]>(embeddedSourceCode)
                .Should()
                .BeEquivalentTo(embeddedFiles);
        }
    }
}
