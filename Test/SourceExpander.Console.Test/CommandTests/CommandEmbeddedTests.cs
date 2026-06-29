using System.Text.Json;

namespace SourceExpander;

[NotInParallel(Initializer.CommandTests)]
public class CommandEmbeddedTests
{
    public static IEnumerable<TestDataRow<(string dllFile, string expected)>> EmbeddedTestData => [
        new(
            DisplayName: "SampleLibrary.dll",
            Data:(Path.Combine(TestUtil.TestProjectDirectory, "tools", "SampleLibrary.dll"),
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
        """)),
        new(
            DisplayName: "SampleLibrary2.dll",
            Data:(Path.Combine(TestUtil.TestProjectDirectory, "tools", "SampleLibrary2.dll"),
        """
        {
            "AssemblyName": "埋込図書館\uD83D\uDCD6",
            "Sources": [
                {
                    "CodeBody": "namespace SampleLibrary{public partial class UnionFind:Dsu{public UnionFind(int n):base(n){Foo();}void Foo()=>Bar();public bool Try(out string?text){if(this.Size(0)==1){text=\"Single\";return true;}text=null;return false;}partial void Bar();}}",
                    "Dependencies": [
                        "ac-library-csharp>Graph/Dsu.cs"
                    ],
                    "FileName": "埋込図書館\uD83D\uDCD6>ionFind.cs",
                    "TypeNames": [
                        "SampleLibrary.UnionFind"
                    ],
                    "Usings": [
                        "using AtCoder;"
                    ]
                },
                {
                    "CodeBody": "namespace SampleLibrary{public static unsafe class UnsafeBlock{public static ulong Convert(double d){double*p=&d;return *(ulong*)p;}}}",
                    "Dependencies": [],
                    "FileName": "埋込図書館\uD83D\uDCD6>safeBlock.cs",
                    "TypeNames": [
                        "SampleLibrary.UnsafeBlock"
                    ],
                    "Usings": []
                }
            ],
            "EmbedderVersion": "9.0.0.100",
            "CSharpVersion": "14.0",
            "AllowUnsafe": true,
            "EmbeddedNamespaces": [
                "SampleLibrary"
            ]
        }
        """)),
        new(
            DisplayName: "SampleLibrary3.dll",
            Data:(Path.Combine(TestUtil.TestProjectDirectory, "tools", "SampleLibrary3.dll"),
        """
        {
            "AssemblyName": "SampleLibrary3",
            "Sources": [
                {
                    "CodeBody": "namespace SampleLibrary{public partial class UnionFind:Dsu{public UnionFind(int n):base(n){Foo();}void Foo()=>Bar();public bool Try(out string?text){if(this.Size(0)==1){text=\"Single\";return true;}text=null;return false;}partial void Bar();}}",
                    "Dependencies": ["ac-library-csharp>Graph/Dsu.cs"],
                    "FileName": "SampleLibrary3>UnionFind.cs",
                    "TypeNames": ["SampleLibrary.UnionFind"],
                    "Usings": ["using AtCoder;"]
                },
                {
                    "CodeBody": "namespace SampleLibrary.D{public static class WithList{public static List<int>Convert(int[]list){return[with([1,2,3]),..list];}}}",
                    "Dependencies": [],
                    "FileName": "SampleLibrary3>WithList.cs",
                    "TypeNames": ["SampleLibrary.D.WithList"],
                    "Usings": ["using System.Collections.Generic;"]
                }
            ],
            "EmbedderVersion": "9.0.0.100",
            "CSharpVersion": "preview",
            "AllowUnsafe": false,
            "EmbeddedNamespaces": ["SampleLibrary","SampleLibrary.D"]
        }
        """)),
    ];

    [Test]
    [MethodDataSource(nameof(EmbeddedTestData))]
    public async Task Embedded(string dllFile, string expected)
    {
        using var sw = new StringWriter();
        await new SourceExpanderCommand { Stdout = sw }.Embedded(dllFile);

        await JsonElement.DeepEquals(JsonElement.Parse(sw.ToString()), JsonElement.Parse(expected)).Should().BeTrue();
    }
}
