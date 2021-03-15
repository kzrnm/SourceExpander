using System.Collections.Immutable;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace SourceExpander.Embedder.Generate.Test
{
    public class PreProcessTest : EmbeddingGeneratorTestBase
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
                     @"class Program{static void Main()=>Console.WriteLine(1);}"
                 ));
            const string embeddedSourceCode = "[{\"CodeBody\":\"class Program{static void Main()=>Console.WriteLine(1);}\",\"Dependencies\":[],\"FileName\":\"TestProject>Program.cs\",\"TypeNames\":[\"Program\"],\"Usings\":[\"using System;\"]}]";

            var test = new Test
            {
                ParseOptions = new CSharpParseOptions(LanguageVersion.CSharp9,
                kind: SourceCodeKind.Regular,
                documentationMode: DocumentationMode.Parse,
                preprocessorSymbols: new[] { "Trace", "TEST" }),
                TestState =
                {
                    AdditionalFiles =
                    {
                        enableMinifyJson,
                    },
                    Sources = {
                        (
                            "/home/source/Program.cs",
                            @"using System;
class Program
{
    static void Main() =>
#if TRACE
    Console.WriteLine(0);
#else
    Console.WriteLine(
#if TEST
1
#else
2
#endif
);
#endif
}
"
                        ),
                    },
                    GeneratedSources =
                    {
                        (typeof(EmbedderGenerator), "EmbeddedSourceCode.Metadata.cs", @$"using System.Reflection;
[assembly: AssemblyMetadataAttribute(""SourceExpander.EmbedderVersion"",""{EmbedderVersion}"")]
[assembly: AssemblyMetadataAttribute(""SourceExpander.EmbeddedLanguageVersion"",""{EmbeddedLanguageVersion}"")]
[assembly: AssemblyMetadataAttribute(""SourceExpander.EmbeddedSourceCode"",{embeddedSourceCode.ToLiteral()})]
"),
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
