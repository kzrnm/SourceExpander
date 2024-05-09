﻿using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace SourceExpander.Embedder
{
    public abstract class EmbedderGeneratorTestBaseWithValue : EmbedderGeneratorTestBase
    {
        public abstract string Syntax { get; }
        public abstract IEnumerable<string> ExpectedTypeNames { get; }
        public abstract IEnumerable<string> ExpectedUsings { get; }
        public abstract IEnumerable<string> ExpectedDependencies { get; }
        public abstract string ExpectedCodeBody { get; }
        public abstract string ExpectedMinifyCodeBody { get; }
        public abstract IEnumerable<string> ExpectedNamespaces { get; }
        public virtual bool ExpectedUnsafe => false;
        public InMemorySourceText Source => new("/foo/path.cs", Syntax);
        internal SourceFileInfo Expected => new(
                    "TestProject>path.cs",
                    ExpectedTypeNames,
                    ExpectedUsings,
                    ExpectedDependencies,
                    ExpectedCodeBody,
                    @unsafe: ExpectedUnsafe);
        internal SourceFileInfo ExpectedMinify => new(
                    "TestProject>path.cs",
                    ExpectedTypeNames,
                    ExpectedUsings,
                    ExpectedDependencies,
                    ExpectedMinifyCodeBody,
                    @unsafe: ExpectedUnsafe);
        public string ExpectedJson => JsonUtil.ToJson(new[] { Expected });
        public string ExpectedMinifyJson => JsonUtil.ToJson(new[] { ExpectedMinify });

        [Fact]
        public async Task Generate()
        {
            var test = new Test
            {
                CompilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true),
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
                    },
                    Sources =
                    {
                        Source,
                    },
                    GeneratedSources =
                    {
                        (typeof(EmbedderGenerator), "EmbeddedSourceCode.Metadata.cs",
                        EnvironmentUtil.JoinByStringBuilder("using System.Reflection;",
                        "[assembly: AssemblyMetadataAttribute(\"SourceExpander.EmbeddedAllowUnsafe\",\"true\")]",
                        $"[assembly: AssemblyMetadataAttribute(\"SourceExpander.EmbedderVersion\",\"{EmbedderVersion}\")]",
                        $"[assembly: AssemblyMetadataAttribute(\"SourceExpander.EmbeddedLanguageVersion\",\"{EmbeddedLanguageVersion}\")]",
                        $"[assembly: AssemblyMetadataAttribute(\"SourceExpander.EmbeddedNamespaces\",\"{string.Join(",", ExpectedNamespaces)}\")]",
                        $"[assembly: AssemblyMetadataAttribute(\"SourceExpander.EmbeddedSourceCode\",{ExpectedJson.ToLiteral()})]")
                        ),
                    },
                }
            };
            await test.RunAsync();
            Newtonsoft.Json.JsonConvert.DeserializeObject<SourceFileInfo[]>(ExpectedJson)
                .Should()
                .ContainSingle()
                .Which
                .Should()
                .BeEquivalentTo(Expected);
            System.Text.Json.JsonSerializer.Deserialize<SourceFileInfo[]>(ExpectedJson)
                .Should()
                .ContainSingle()
                .Which
                .Should()
                .BeEquivalentTo(Expected);
        }

        [Fact]
        public async Task Minify()
        {
            var test = new Test
            {
                CompilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true),
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
                    },
                    Sources =
                    {
                        Source,
                    },
                    GeneratedSources =
                    {
                        (typeof(EmbedderGenerator), "EmbeddedSourceCode.Metadata.cs",
                        EnvironmentUtil.JoinByStringBuilder("using System.Reflection;",
                        "[assembly: AssemblyMetadataAttribute(\"SourceExpander.EmbeddedAllowUnsafe\",\"true\")]",
                        $"[assembly: AssemblyMetadataAttribute(\"SourceExpander.EmbedderVersion\",\"{EmbedderVersion}\")]",
                        $"[assembly: AssemblyMetadataAttribute(\"SourceExpander.EmbeddedLanguageVersion\",\"{EmbeddedLanguageVersion}\")]",
                        $"[assembly: AssemblyMetadataAttribute(\"SourceExpander.EmbeddedNamespaces\",\"{string.Join(",", ExpectedNamespaces)}\")]",
                        $"[assembly: AssemblyMetadataAttribute(\"SourceExpander.EmbeddedSourceCode\",{ExpectedMinifyJson.ToLiteral()})]")
                        ),
                    },
                }
            };
            await test.RunAsync();
            Newtonsoft.Json.JsonConvert.DeserializeObject<SourceFileInfo[]>(ExpectedMinifyJson)
                .Should()
                .ContainSingle()
                .Which
                .Should()
                .BeEquivalentTo(ExpectedMinify);
            System.Text.Json.JsonSerializer.Deserialize<SourceFileInfo[]>(ExpectedMinifyJson)
                .Should()
                .ContainSingle()
                .Which
                .Should()
                .BeEquivalentTo(ExpectedMinify);
        }
    }
}
