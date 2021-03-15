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
            const string embeddedSourceCode = "㘅桠ҠҠҠ伖㿂⢃㹈㝟謝櫴⫢ꃵ鮱䯧腪䃺亇溦䫉峉㚬卤齏浑懆ᒾꑉ疱吧灊Ң遰䲑廛䐬痯㻼ꃲ鞓磂駄澚䱽儺㐛မ鐨頪覉杆琪ҹ疙盈䂅䩨卷圢頇ᅖ䉱捭羧鯣讫ꗟ奃泲嫀鏿⣀藀彷迤豜緜酲跀渑棗悿癲䣇ᕊ詠Ҡ";

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
[assembly: AssemblyMetadataAttribute(""SourceExpander.EmbeddedSourceCode.GZipBase32768"",""{embeddedSourceCode}"")]
"),
                    }
                }
            };
            await test.RunAsync();
            Newtonsoft.Json.JsonConvert.DeserializeObject<SourceFileInfo[]>(
                SourceFileInfoUtil.FromGZipBase32768(embeddedSourceCode))
                .Should()
                .BeEquivalentTo(embeddedFiles);
            System.Text.Json.JsonSerializer.Deserialize<SourceFileInfo[]>(
                SourceFileInfoUtil.FromGZipBase32768(embeddedSourceCode))
                .Should()
                .BeEquivalentTo(embeddedFiles);
        }
    }
}
