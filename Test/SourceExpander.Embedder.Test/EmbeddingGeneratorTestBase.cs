using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;

namespace SourceExpander.Embedder
{
    public class EmbeddingGeneratorTestBase
    {
        public class Test : CSharpSourceGeneratorTest<EmbedderGenerator>
        {
            public Test()
            {
                ParseOptions = ParseOptions.WithLanguageVersion(EmbeddedLanguageVersionEnum);
                ReferenceAssemblies = ReferenceAssemblies.Net.Net50.AddPackages(Packages);
                foreach (var (hintName, sourceText) in CompileTimeTypeMaker.Sources)
                {
                    TestState.GeneratedSources.Add((typeof(EmbedderGenerator), hintName, sourceText));
                }
            }
        }
        internal static ImmutableArray<PackageIdentity> Packages
            = ImmutableArray.Create(new PackageIdentity("SourceExpander.Core", "2.6.0"));

        public static string EmbedderVersion => typeof(EmbedderGenerator).Assembly.GetName().Version.ToString();
        private static readonly LanguageVersion EmbeddedLanguageVersionEnum = LanguageVersion.CSharp9;
        public static string EmbeddedLanguageVersion => EmbeddedLanguageVersionEnum.ToDisplayString();

        public static InMemorySourceText enableMinifyJson = new(
            "/foo/bar/SourceExpander.Embedder.Config.json", @"
{
    ""$schema"": ""https://raw.githubusercontent.com/naminodarie/SourceExpander/master/schema/embedder.schema.json"",
    ""embedding-type"": ""Raw"",
    ""enable-minify"": true
}
");
    }
}
