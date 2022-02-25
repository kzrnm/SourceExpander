using System.Collections.Immutable;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace SourceExpander.Generate
{
    public class UsingDirectiveTest : EmbedderGeneratorTestBase
    {
        [Fact]
        public async Task Generate()
        {
            var embeddedNamespaces = ImmutableArray<string>.Empty;
            var embeddedFiles = ImmutableArray.Create(
                 new SourceFileInfo
                 (
                     "TestProject>Program.cs",
                     new string[] { "Program" },
                     ImmutableArray.Create("using List = System.Collections.Generic.List<int>;", "using System;", "using static System.Math;"),
                     ImmutableArray<string>.Empty,
                     @"class Program{static void Main()=>Console.WriteLine(0);static List L(int x,int y)=>new List(Min(x,y));}"
                 ));

            const string embeddedSourceCode = "[{\"CodeBody\":\"class Program{static void Main()=>Console.WriteLine(0);static List L(int x,int y)=>new List(Min(x,y));}\",\"Dependencies\":[],\"FileName\":\"TestProject>Program.cs\",\"TypeNames\":[\"Program\"],\"Usings\":[\"using List = System.Collections.Generic.List<int>;\",\"using System;\",\"using static System.Math;\"]}]";

            var test = new Test
            {
                TestState =
                {
                    AdditionalFiles = { enableMinifyJson },
                    Sources = {
                        (
                            "/home/source/Program.cs",
                            @"using System;
using static System.Math;
using List = System.Collections.Generic.List<int>;
class Program
{
    static void Main() => Console.WriteLine(0);
    static List L(int x, int y) => new List(Min(x, y));
}
"
                        ),
                    },
                    ExpectedDiagnostics =
                    {
                        new DiagnosticResult("EMBED0009", DiagnosticSeverity.Info).WithSpan("/home/source/Program.cs", 2, 1, 2, 26),
                        new DiagnosticResult("EMBED0010", DiagnosticSeverity.Info).WithSpan("/home/source/Program.cs", 3, 1, 3, 51),

                    },
                    GeneratedSources =
                    {
                        (typeof(EmbedderGenerator), "EmbeddedSourceCode.Metadata.cs",
                        EnvironmentUtil.JoinByStringBuilder("using System.Reflection;",
                        $"[assembly: AssemblyMetadataAttribute(\"SourceExpander.EmbedderVersion\",\"{EmbedderVersion}\")]",
                        $"[assembly: AssemblyMetadataAttribute(\"SourceExpander.EmbeddedLanguageVersion\",\"{EmbeddedLanguageVersion}\")]",
                        $"[assembly: AssemblyMetadataAttribute(\"SourceExpander.EmbeddedNamespaces\",\"{string.Join(",", embeddedNamespaces)}\")]",
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
