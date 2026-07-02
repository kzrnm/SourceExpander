using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace SourceExpander.Embedder.Syntaxes;

public abstract class SyntaxKindTestBase : EmbedderGeneratorTestBase
{
    public abstract string Syntax { get; }
    public abstract ImmutableArray<string> ExpectedTypeNames { get; }
    public abstract ImmutableArray<string> ExpectedUsings { get; }
    public abstract ImmutableArray<string> ExpectedDependencies { get; }
    public abstract string ExpectedCodeBody { get; }
    public abstract string ExpectedMinifyCodeBody { get; }
    public abstract ImmutableArray<string> ExpectedNamespaces { get; }
    public InMemorySourceText Source => new("/foo/path.cs", Syntax);
    internal SourceFileInfo Expected => new(
                "TestProject>path.cs",
                ExpectedTypeNames,
                ExpectedUsings,
                ExpectedDependencies,
                ExpectedCodeBody);
    internal SourceFileInfo ExpectedMinify => new(
                "TestProject>path.cs",
                ExpectedTypeNames,
                ExpectedUsings,
                ExpectedDependencies,
                ExpectedMinifyCodeBody);
    public string ExpectedJson => JsonUtil.ToJson(new[] { Expected });
    public string ExpectedMinifyJson => JsonUtil.ToJson(new[] { ExpectedMinify });

    [Test]
    public async Task Generate(CancellationToken cancellationToken)
    {
        await ExpectedJson.Should().BeEquivalentToJsonSources([Expected]);
        var test = new Test(new()
        {
            AllowUnsafe = true,
            EmbeddedNamespaces = string.Join(",", ExpectedNamespaces),
            EmbeddedSourceCode = ExpectedJson,
        })
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
            }
        };
        await test.RunAsync(cancellationToken);
        await Newtonsoft.Json.JsonConvert.DeserializeObject<SourceFileInfo[]>(ExpectedJson)
            .Should()
            .BeEquivalentTo([Expected], TestUtil.SourceFileInfoEqualityComparer);
        await System.Text.Json.JsonSerializer.Deserialize<SourceFileInfo[]>(ExpectedJson)
            .Should()
            .BeEquivalentTo([Expected], TestUtil.SourceFileInfoEqualityComparer);
    }

    [Test]
    public async Task Minify(CancellationToken cancellationToken)
    {
        await ExpectedMinifyJson.Should().BeEquivalentToJsonSources([ExpectedMinify]);
        var test = new Test(new()
        {
            AllowUnsafe = true,
            EmbeddedNamespaces = string.Join(",", ExpectedNamespaces),
            EmbeddedSourceCode = ExpectedMinifyJson,
        })
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
            }
        };
        await test.RunAsync(cancellationToken);
    }
}
