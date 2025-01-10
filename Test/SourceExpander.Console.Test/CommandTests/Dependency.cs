using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace SourceExpander;

public partial class CommandTests
{
    [Fact]
    public async Task DependencySampleAppSkipAtcoder()
    {
        using var sw = new StringWriter();
        var project = Path.Combine(TestUtil.TestProjectDirectory, "tools", "SampleAppSkipAtcoder.csproj");
        await new SourceExpanderCommand { Stdout = sw }.Dependency(project, cancellationToken: TestContext.Current.CancellationToken);

        var obj = JsonSerializer.Deserialize<DependencyResult[]>(sw.ToString());
        obj.Should().NotBeNull();
        var dic = obj!.ToDictionary(e => e.FileName);

        dic.Should().ContainKeys([
            Path.Combine(TestUtil.SourceDirectory, "Sandbox", "SampleApp", "Program.cs"),
            Path.Combine(TestUtil.SourceDirectory, "Sandbox", "SampleApp", "Program2.cs"),
            "SampleLibrary>ionFind.cs",
            "SampleLibrary>safeBlock.cs", // `Un` is common prefix!
        ])
            .And.NotContainKeys([
            "ac-library-csharp>Graph/Dsu.cs",
            ]);

        dic[Path.Combine(TestUtil.SourceDirectory, "Sandbox", "SampleApp", "Program.cs")].TypeNames.Should().BeEmpty();
        dic[Path.Combine(TestUtil.SourceDirectory, "Sandbox", "SampleApp", "Program.cs")].Dependencies.Should().BeEquivalentTo([
            "SampleLibrary>ionFind.cs"
        ]);
        dic[Path.Combine(TestUtil.SourceDirectory, "Sandbox", "SampleApp", "Program2.cs")].TypeNames.Should().BeEmpty();
        dic[Path.Combine(TestUtil.SourceDirectory, "Sandbox", "SampleApp", "Program2.cs")].Dependencies.Should().BeEquivalentTo([
            "SampleLibrary>safeBlock.cs"
        ]);
        dic["SampleLibrary>ionFind.cs"].TypeNames.Should().BeEquivalentTo(["SampleLibrary.UnionFind"]);
        dic["SampleLibrary>ionFind.cs"].Dependencies.Should().BeEquivalentTo(["ac-library-csharp>Graph/Dsu.cs"]);
        dic["SampleLibrary>safeBlock.cs"].TypeNames.Should().BeEquivalentTo(["SampleLibrary.UnsafeBlock"]);
        dic["SampleLibrary>safeBlock.cs"].Dependencies.Should().BeEquivalentTo([]);
    }

    [Fact]
    public async Task DependencySampleApp()
    {
        using var sw = new StringWriter();
        var project = Path.Combine(TestUtil.SourceDirectory, "Sandbox", "SampleApp", "SampleApp.csproj");
        await new SourceExpanderCommand { Stdout = sw }.Dependency(project, cancellationToken: TestContext.Current.CancellationToken);

        var obj = JsonSerializer.Deserialize<DependencyResult[]>(sw.ToString());
        obj.Should().NotBeNull();
        var dic = obj!.ToDictionary(e => e.FileName);

        dic.Should().ContainKeys([
            Path.Combine(TestUtil.SourceDirectory, "Sandbox", "SampleApp", "Program.cs"),
            Path.Combine(TestUtil.SourceDirectory, "Sandbox", "SampleApp", "Program2.cs"),
            "ac-library-csharp>Graph/Dsu.cs",
            "SampleLibrary>ionFind.cs",
            "SampleLibrary>safeBlock.cs", // `Un` is common prefix!
        ]);

        dic[Path.Combine(TestUtil.SourceDirectory, "Sandbox", "SampleApp", "Program.cs")].TypeNames.Should().BeEmpty();
        dic[Path.Combine(TestUtil.SourceDirectory, "Sandbox", "SampleApp", "Program.cs")].Dependencies.Should().BeEquivalentTo([
            "ac-library-csharp>Graph/Dsu.cs",
            "ac-library-csharp>Internal/SimpleList.cs",
            "SampleLibrary>ionFind.cs"
        ]);
        dic[Path.Combine(TestUtil.SourceDirectory, "Sandbox", "SampleApp", "Program2.cs")].TypeNames.Should().BeEmpty();
        dic[Path.Combine(TestUtil.SourceDirectory, "Sandbox", "SampleApp", "Program2.cs")].Dependencies.Should().BeEquivalentTo([
            "SampleLibrary>safeBlock.cs"
        ]);
        dic["ac-library-csharp>Graph/Dsu.cs"].TypeNames.Should().BeEquivalentTo(["AtCoder.Dsu"]);
        dic["ac-library-csharp>Graph/Dsu.cs"].Dependencies.Should().BeEquivalentTo(["ac-library-csharp>Internal/SimpleList.cs"]);
        dic["SampleLibrary>ionFind.cs"].TypeNames.Should().BeEquivalentTo(["SampleLibrary.UnionFind"]);
        dic["SampleLibrary>ionFind.cs"].Dependencies.Should().BeEquivalentTo(["ac-library-csharp>Graph/Dsu.cs"]);
        dic["SampleLibrary>safeBlock.cs"].TypeNames.Should().BeEquivalentTo(["SampleLibrary.UnsafeBlock"]);
        dic["SampleLibrary>safeBlock.cs"].Dependencies.Should().BeEquivalentTo([]);
    }

    [Fact]
    public async Task DependencySampleLibrary()
    {
        using var sw = new StringWriter();
        var project = Path.Combine(TestUtil.SourceDirectory, "Sandbox", "SampleLibrary", "SampleLibrary.csproj");
        await new SourceExpanderCommand { Stdout = sw }.Dependency(project, cancellationToken: TestContext.Current.CancellationToken);

        var obj = JsonSerializer.Deserialize<DependencyResult[]>(sw.ToString());
        obj.Should().NotBeNull();
        var dic = obj!.ToDictionary(e => e.FileName);

        dic.Should().ContainKeys([
            "ac-library-csharp>Graph/Dsu.cs",
            "SampleLibrary>ionFind.cs",
            "SampleLibrary>safeBlock.cs", // `Un` is common prefix!
        ]);

        dic["ac-library-csharp>Graph/Dsu.cs"].TypeNames.Should().BeEquivalentTo(["AtCoder.Dsu"]);
        dic["ac-library-csharp>Graph/Dsu.cs"].Dependencies.Should().BeEquivalentTo(["ac-library-csharp>Internal/SimpleList.cs"]);
        dic["SampleLibrary>ionFind.cs"].TypeNames.Should().BeEquivalentTo(["SampleLibrary.UnionFind"]);
        dic["SampleLibrary>ionFind.cs"].Dependencies.Should().BeEquivalentTo(["ac-library-csharp>Graph/Dsu.cs"]);
        dic["SampleLibrary>safeBlock.cs"].TypeNames.Should().BeEquivalentTo(["SampleLibrary.UnsafeBlock"]);
        dic["SampleLibrary>safeBlock.cs"].Dependencies.Should().BeEquivalentTo([]);
    }


    [Fact]
    public async Task DependencyFullFilePath()
    {
        using var sw = new StringWriter();
        var project = Path.Combine(TestUtil.SourceDirectory, "Sandbox", "SampleLibrary", "SampleLibrary.csproj");
        await new SourceExpanderCommand { Stdout = sw }.Dependency(project, fullFilePath: true, cancellationToken: TestContext.Current.CancellationToken);

        var obj = JsonSerializer.Deserialize<DependencyResult[]>(sw.ToString());
        obj.Should().NotBeNull();
        var dic = obj!.ToDictionary(e => e.FileName);

        dic.Should().ContainKeys([
            "ac-library-csharp>Graph/Dsu.cs",
            Path.Combine(TestUtil.SourceDirectory, "Sandbox", "SampleLibrary", "UnionFind.cs"),
            Path.Combine(TestUtil.SourceDirectory, "Sandbox", "SampleLibrary", "UnsafeBlock.cs"),
        ]);

        dic["ac-library-csharp>Graph/Dsu.cs"].TypeNames.Should().BeEquivalentTo(["AtCoder.Dsu"]);
        dic["ac-library-csharp>Graph/Dsu.cs"].Dependencies.Should().BeEquivalentTo(["ac-library-csharp>Internal/SimpleList.cs"]);
        dic[Path.Combine(TestUtil.SourceDirectory, "Sandbox", "SampleLibrary", "UnionFind.cs")].TypeNames.Should().BeEquivalentTo(["SampleLibrary.UnionFind"]);
        dic[Path.Combine(TestUtil.SourceDirectory, "Sandbox", "SampleLibrary", "UnionFind.cs")].Dependencies.Should().BeEquivalentTo(["ac-library-csharp>Graph/Dsu.cs"]);
        dic[Path.Combine(TestUtil.SourceDirectory, "Sandbox", "SampleLibrary", "UnsafeBlock.cs")].TypeNames.Should().BeEquivalentTo(["SampleLibrary.UnsafeBlock"]);
        dic[Path.Combine(TestUtil.SourceDirectory, "Sandbox", "SampleLibrary", "UnsafeBlock.cs")].Dependencies.Should().BeEquivalentTo([]);
    }
}
