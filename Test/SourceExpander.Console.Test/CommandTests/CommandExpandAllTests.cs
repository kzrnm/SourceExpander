using System.Text.Json;

namespace SourceExpander;

[NotInParallel(Initializer.CommandTests)]
public class CommandExpandAllTests
{
    [Test]
    public async Task ExpandAll(CancellationToken cancellationToken)
    {
        using var sw = new StringWriter();
        var project = Path.Combine(TestUtil.TestProjectDirectory, "tools", "SampleAppSkipAtcoder.csproj");
        await new SourceExpanderCommand { Stdout = sw }.ExpandAll(project, cancellationToken: cancellationToken);

        var obj = JsonSerializer.Deserialize<ExpandAllObject[]>(sw.ToString());
        await obj.Should().NotBeNull();
        var dic = obj!.ToDictionary(e => e.FilePath);

        await dic.Should().ContainKey(Path.Combine(TestUtil.SourceDirectory, "Sandbox", "SampleApp", "Program.cs"));
        await dic[Path.Combine(TestUtil.SourceDirectory, "Sandbox", "SampleApp", "Program.cs")].ExpandedCode.ReplaceLineEndings().Should().BeEqualTo("""
using AtCoder;
using SampleLibrary;
using System;
namespace SampleApp
{
    class Program
    {
        static void Main()
        {
            var uf = new UnionFind(3);
            uf.Merge(1, 2);
            Console.WriteLine(uf.Leader(2));
        }
    }
}
#region Expanded by https://github.com/kzrnm/SourceExpander
namespace SampleLibrary { public partial class UnionFind : Dsu { public UnionFind(int n) : base(n) { Foo(); }  void Foo() => Bar(); public bool Try(out string? text) { if (this.Size(0) == 1) { text = "Single"; return true; }  text = null; return false; }  partial void Bar(); } }
#endregion Expanded by https://github.com/kzrnm/SourceExpander

""".ReplaceLineEndings());

        await dic.Should().ContainKey(Path.Combine(TestUtil.SourceDirectory, "Sandbox", "SampleApp", "Program2.cs"));
        await dic[Path.Combine(TestUtil.SourceDirectory, "Sandbox", "SampleApp", "Program2.cs")].ExpandedCode.ReplaceLineEndings().Should().BeEqualTo("""
using SampleLibrary;
using System;
namespace SampleApp
{
    class Program2
    {
        static void Main()
        {
            Console.WriteLine(UnsafeBlock.Convert(3.14));
        }
    }
}
#region Expanded by https://github.com/kzrnm/SourceExpander
namespace SampleLibrary { public static unsafe class UnsafeBlock { public static ulong Convert(double d) { double* p = &d; return *(ulong*)p; } } }
#endregion Expanded by https://github.com/kzrnm/SourceExpander

""".ReplaceLineEndings());
    }

    [Test]
    public async Task ExpandAllWithStaticEmbedding(CancellationToken cancellationToken)
    {
        using var sw = new StringWriter();
        var project = Path.Combine(TestUtil.TestProjectDirectory, "tools", "SampleAppSkipAtcoder.csproj");
        await new SourceExpanderCommand { Stdout = sw }.ExpandAll(project, staticEmbedding: "/* 🥇 */", cancellationToken: cancellationToken);

        var obj = JsonSerializer.Deserialize<ExpandAllObject[]>(sw.ToString());
        await obj.Should().NotBeNull();
        var dic = obj!.ToDictionary(e => e.FilePath);

        await dic.Should().ContainKey(Path.Combine(TestUtil.SourceDirectory, "Sandbox", "SampleApp", "Program.cs"));
        await dic[Path.Combine(TestUtil.SourceDirectory, "Sandbox", "SampleApp", "Program.cs")].ExpandedCode.ReplaceLineEndings().Should().BeEqualTo("""
using AtCoder;
using SampleLibrary;
using System;
namespace SampleApp
{
    class Program
    {
        static void Main()
        {
            var uf = new UnionFind(3);
            uf.Merge(1, 2);
            Console.WriteLine(uf.Leader(2));
        }
    }
}
#region Expanded by https://github.com/kzrnm/SourceExpander
/* 🥇 */
namespace SampleLibrary { public partial class UnionFind : Dsu { public UnionFind(int n) : base(n) { Foo(); }  void Foo() => Bar(); public bool Try(out string? text) { if (this.Size(0) == 1) { text = "Single"; return true; }  text = null; return false; }  partial void Bar(); } }
#endregion Expanded by https://github.com/kzrnm/SourceExpander

""".ReplaceLineEndings());

        await dic.Should().ContainKey(Path.Combine(TestUtil.SourceDirectory, "Sandbox", "SampleApp", "Program2.cs"));
        await dic[Path.Combine(TestUtil.SourceDirectory, "Sandbox", "SampleApp", "Program2.cs")].ExpandedCode.ReplaceLineEndings().Should().BeEqualTo("""
using SampleLibrary;
using System;
namespace SampleApp
{
    class Program2
    {
        static void Main()
        {
            Console.WriteLine(UnsafeBlock.Convert(3.14));
        }
    }
}
#region Expanded by https://github.com/kzrnm/SourceExpander
/* 🥇 */
namespace SampleLibrary { public static unsafe class UnsafeBlock { public static ulong Convert(double d) { double* p = &d; return *(ulong*)p; } } }
#endregion Expanded by https://github.com/kzrnm/SourceExpander

""".ReplaceLineEndings());
    }
    record ExpandAllObject(string FilePath, string ExpandedCode);
}
