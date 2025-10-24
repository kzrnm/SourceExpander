using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace SourceExpander;

[Collection(Initializer.CommandTests)]
public class CommandExpandAllTests
{
    [Fact]
    public async Task ExpandAll()
    {
        using var sw = new StringWriter();
        var project = Path.Combine(TestUtil.TestProjectDirectory, "tools", "SampleAppSkipAtcoder.csproj");
        await new SourceExpanderCommand { Stdout = sw }.ExpandAll(project, cancellationToken: TestContext.Current.CancellationToken);

        var obj = JsonSerializer.Deserialize<ExpandAllObject[]>(sw.ToString());
        obj.ShouldNotBeNull();
        var dic = obj!.ToDictionary(e => e.FilePath);

        dic.ShouldContainKey(Path.Combine(TestUtil.SourceDirectory, "Sandbox", "SampleApp", "Program.cs"));
        dic[Path.Combine(TestUtil.SourceDirectory, "Sandbox", "SampleApp", "Program.cs")].ExpandedCode.ReplaceLineEndings().ShouldBe("""
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

        dic.ShouldContainKey(Path.Combine(TestUtil.SourceDirectory, "Sandbox", "SampleApp", "Program2.cs"));
        dic[Path.Combine(TestUtil.SourceDirectory, "Sandbox", "SampleApp", "Program2.cs")].ExpandedCode.ReplaceLineEndings().ShouldBe("""
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

    [Fact]
    public async Task ExpandAllWithStaticEmbedding()
    {
        using var sw = new StringWriter();
        var project = Path.Combine(TestUtil.TestProjectDirectory, "tools", "SampleAppSkipAtcoder.csproj");
        await new SourceExpanderCommand { Stdout = sw }.ExpandAll(project, staticEmbedding: "/* 🥇 */", cancellationToken: TestContext.Current.CancellationToken);

        var obj = JsonSerializer.Deserialize<ExpandAllObject[]>(sw.ToString());
        obj.ShouldNotBeNull();
        var dic = obj!.ToDictionary(e => e.FilePath);

        dic.ShouldContainKey(Path.Combine(TestUtil.SourceDirectory, "Sandbox", "SampleApp", "Program.cs"));
        dic[Path.Combine(TestUtil.SourceDirectory, "Sandbox", "SampleApp", "Program.cs")].ExpandedCode.ReplaceLineEndings().ShouldBe("""
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

        dic.ShouldContainKey(Path.Combine(TestUtil.SourceDirectory, "Sandbox", "SampleApp", "Program2.cs"));
        dic[Path.Combine(TestUtil.SourceDirectory, "Sandbox", "SampleApp", "Program2.cs")].ExpandedCode.ReplaceLineEndings().ShouldBe("""
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
