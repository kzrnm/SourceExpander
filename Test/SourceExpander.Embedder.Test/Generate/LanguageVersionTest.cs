﻿using System.Collections.Immutable;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace SourceExpander.Generate
{
    public class LanguageVersionTest : EmbedderGeneratorTestBase
    {
        [Theory]
        [InlineData(LanguageVersion.Latest)]
        [InlineData(LanguageVersion.LatestMajor)]
        [InlineData(LanguageVersion.Preview)]
        [InlineData(LanguageVersion.CSharp7_3)]
        [InlineData(LanguageVersion.CSharp8)]
        [InlineData(LanguageVersion.CSharp9)]
        [InlineData(LanguageVersion.CSharp10)]
        public async Task Generate(LanguageVersion languageVersion)
        {
            var embeddedNamespaces = ImmutableArray<string>.Empty;
            var embeddedFiles = ImmutableArray.Create(
                 new SourceFileInfo
                 (
                     "TestProject>Program.cs",
                     ["Program"],
                     ImmutableArray.Create("using System;"),
                     ImmutableArray<string>.Empty,
                     @"class Program{static void Main()=>Console.WriteLine(1);}"
                 ));
            const string embeddedSourceCode = "[{\"CodeBody\":\"class Program{static void Main()=>Console.WriteLine(1);}\",\"Dependencies\":[],\"FileName\":\"TestProject>Program.cs\",\"TypeNames\":[\"Program\"],\"Usings\":[\"using System;\"]}]";

            var test = new Test
            {
                ParseOptions = new CSharpParseOptions(languageVersion),
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
    static void Main() => Console.WriteLine(1);
}
"
                        ),
                    },
                    GeneratedSources =
                    {
                        (typeof(EmbedderGenerator), "EmbeddedSourceCode.Metadata.cs",
                        EnvironmentUtil.JoinByStringBuilder(
                        "// <auto-generated/>",
                        "#pragma warning disable",
                        $"[assembly: global::System.Reflection.AssemblyMetadataAttribute(\"SourceExpander.EmbedderVersion\",\"{EmbedderVersion}\")]",
                        $"[assembly: global::System.Reflection.AssemblyMetadataAttribute(\"SourceExpander.EmbeddedLanguageVersion\",\"{languageVersion.MapSpecifiedToEffectiveVersion().ToDisplayString()}\")]",
                        $"[assembly: global::System.Reflection.AssemblyMetadataAttribute(\"SourceExpander.EmbeddedNamespaces\",\"{string.Join(",", embeddedNamespaces)}\")]",
                        $"[assembly: global::System.Reflection.AssemblyMetadataAttribute(\"SourceExpander.EmbeddedSourceCode\",{embeddedSourceCode.ToLiteral()})]")
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
