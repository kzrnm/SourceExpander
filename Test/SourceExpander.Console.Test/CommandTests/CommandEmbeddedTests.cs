using System.Text.Json;

namespace SourceExpander;

[NotInParallel(Initializer.CommandTests)]
public class CommandEmbeddedTests
{

    [Test]
    [Arguments(null)]
    [Arguments("./SampleLibraryRemovedMetadata.dll")]
    public async Task Embedded(string? output)
    {
        using var sw = new StringWriter();
        var dllFile = Path.Combine(TestUtil.TestProjectDirectory, "tools", "SampleLibrary.dll");
        await new SourceExpanderCommand { Stdout = sw }.Embedded(dllFile, output: output);

        await JsonElement.DeepEquals(JsonElement.Parse(sw.ToString()), JsonElement.Parse(
        """
        {
            "AssemblyName": "SampleLibrary",
            "EmbedderVersion": "8.3.0.100",
            "CSharpVersion": "14.0",
            "AllowUnsafe": true,
            "EmbeddedNamespaces": ["SampleLibrary"],
            "Sources": [
                {
                    "CodeBody": "namespace SampleLibrary { public partial class UnionFind : Dsu { public UnionFind(int n) : base(n) { Foo(); }  void Foo() => Bar(); public bool Try(out string? text) { if (this.Size(0) == 1) { text = \"Single\"; return true; }  text = null; return false; }  partial void Bar(); } }",
                    "Dependencies": ["ac-library-csharp>Graph/Dsu.cs"],
                    "FileName": "SampleLibrary>nionFind.cs",
                    "TypeNames": ["SampleLibrary.UnionFind"],
                    "Usings": ["using AtCoder;"]
                },
                {
                    "CodeBody": "namespace SampleLibrary { public static unsafe class UnsafeBlock { public static ulong Convert(double d) { double* p = &d; return *(ulong*)p; } } }",
                    "Dependencies": [],
                    "FileName": "SampleLibrary>nsafeBlock.cs",
                    "TypeNames": ["SampleLibrary.UnsafeBlock"],
                    "Usings": []
                },
                {
                    "CodeBody": "namespace SampleLibrary { internal class UseEmbeddingOnly { [Conditional(\"SOURCE_EMBEDDING\")] public static void EmbeddingExample() { } } }",
                    "Dependencies": [],
                    "FileName": "SampleLibrary>seEmbeddingOnly.cs",
                    "TypeNames": ["SampleLibrary.UseEmbeddingOnly"],
                    "Usings": ["using System.Diagnostics;"]
                }
            ]
        }
        """)).Should().BeTrue();

        if (output is not null)
        {
            sw.GetStringBuilder().Clear();
            await Path.Exists(output).Should().BeTrue();
            await new SourceExpanderCommand { Stdout = sw }.Embedded(output);
            await JsonElement.DeepEquals(JsonElement.Parse(sw.ToString()), JsonElement.Parse(
            """
            {
                "AssemblyName": "SampleLibrary",
                "EmbedderVersion": "1.0.0",
                "CSharpVersion": "1",
                "AllowUnsafe": false,
                "EmbeddedNamespaces": [],
                "Sources": []
            }
            """)).Should().BeTrue();
        }
    }
}
