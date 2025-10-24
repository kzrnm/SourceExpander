using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace SourceExpander;

[Collection(Initializer.CommandTests)]
public class CommandDependencyTests
{
    [Fact]
    public async Task DependencySampleAppSkipAtcoder()
    {
        using var sw = new StringWriter();
        var project = Path.Combine(TestUtil.TestProjectDirectory, "tools", "SampleAppSkipAtcoder.csproj");
        await new SourceExpanderCommand { Stdout = sw }.Dependency(project, cancellationToken: TestContext.Current.CancellationToken);

        var obj = JsonSerializer.Deserialize<DependencyResult[]>(sw.ToString());
        obj.ShouldNotBeNull();
        var dic = obj!.ToDictionary(e => e.FileName);

        dic.ShouldContainKeyAndValue(Path.Combine(TestUtil.SourceDirectory, "Sandbox", "SampleApp", "Program.cs"),
            new(Path.Combine(TestUtil.SourceDirectory, "Sandbox", "SampleApp", "Program.cs"),
            [
            "SampleLibrary>ionFind.cs",
            ],
            []));

        dic.ShouldContainKeyAndValue(Path.Combine(TestUtil.SourceDirectory, "Sandbox", "SampleApp", "Program2.cs"),
            new(Path.Combine(TestUtil.SourceDirectory, "Sandbox", "SampleApp", "Program2.cs"),
            ["SampleLibrary>safeBlock.cs"],
            []));

        dic.ShouldNotContainKey("ac-library-csharp>Graph/Dsu.cs");

        // `Un` is common prefix!
        dic.ShouldContainKeyAndValue("SampleLibrary>ionFind.cs",
            new("SampleLibrary>ionFind.cs",
            ["ac-library-csharp>Graph/Dsu.cs"],
            ["SampleLibrary.UnionFind"]));

        // `Un` is common prefix!
        dic.ShouldContainKeyAndValue("SampleLibrary>safeBlock.cs",
            new("SampleLibrary>safeBlock.cs",
            [],
            ["SampleLibrary.UnsafeBlock"]));
    }

    [Fact]
    public async Task DependencySampleApp()
    {
        using var sw = new StringWriter();
        var project = Path.Combine(TestUtil.SourceDirectory, "Sandbox", "SampleApp", "SampleApp.csproj");
        await new SourceExpanderCommand { Stdout = sw }.Dependency(project, cancellationToken: TestContext.Current.CancellationToken);

        var obj = JsonSerializer.Deserialize<DependencyResult[]>(sw.ToString());
        obj.ShouldNotBeNull();
        var dic = obj!.ToDictionary(e => e.FileName);


        dic.ShouldContainKeyAndValue(Path.Combine(TestUtil.SourceDirectory, "Sandbox", "SampleApp", "Program.cs"),
            new(Path.Combine(TestUtil.SourceDirectory, "Sandbox", "SampleApp", "Program.cs"),
            [
            "ac-library-csharp>Graph/Dsu.cs",
            "ac-library-csharp>Internal/SimpleList.cs",
            "SampleLibrary>ionFind.cs",
            ],
            []));

        dic.ShouldContainKeyAndValue(Path.Combine(TestUtil.SourceDirectory, "Sandbox", "SampleApp", "Program2.cs"),
            new(Path.Combine(TestUtil.SourceDirectory, "Sandbox", "SampleApp", "Program2.cs"),
            ["SampleLibrary>safeBlock.cs"],
            []));

        dic.ShouldContainKeyAndValue("ac-library-csharp>Graph/Dsu.cs",
            new("ac-library-csharp>Graph/Dsu.cs",
            ["ac-library-csharp>Internal/SimpleList.cs"],
            ["AtCoder.Dsu"]));

        // `Un` is common prefix!
        dic.ShouldContainKeyAndValue("SampleLibrary>ionFind.cs",
            new("SampleLibrary>ionFind.cs",
            ["ac-library-csharp>Graph/Dsu.cs"],
            ["SampleLibrary.UnionFind"]));

        // `Un` is common prefix!
        dic.ShouldContainKeyAndValue("SampleLibrary>safeBlock.cs",
            new("SampleLibrary>safeBlock.cs",
            [],
            ["SampleLibrary.UnsafeBlock"]));
    }

    [Fact]
    public async Task DependencySampleLibrary()
    {
        using var sw = new StringWriter();
        var project = Path.Combine(TestUtil.SourceDirectory, "Sandbox", "SampleLibrary", "SampleLibrary.csproj");
        await new SourceExpanderCommand { Stdout = sw }.Dependency(project, cancellationToken: TestContext.Current.CancellationToken);

        var obj = JsonSerializer.Deserialize<DependencyResult[]>(sw.ToString());
        obj.ShouldNotBeNull();
        var dic = obj!.ToDictionary(e => e.FileName);

        dic.ShouldContainKeyAndValue("ac-library-csharp>Graph/Dsu.cs",
            new("ac-library-csharp>Graph/Dsu.cs",
            ["ac-library-csharp>Internal/SimpleList.cs"],
            ["AtCoder.Dsu"]));

        // `Un` is common prefix!
        dic.ShouldContainKeyAndValue("SampleLibrary>ionFind.cs",
            new("SampleLibrary>ionFind.cs",
            ["ac-library-csharp>Graph/Dsu.cs"],
            ["SampleLibrary.UnionFind"]));

        // `Un` is common prefix!
        dic.ShouldContainKeyAndValue("SampleLibrary>safeBlock.cs",
            new("SampleLibrary>safeBlock.cs",
            [],
            ["SampleLibrary.UnsafeBlock"]));
    }


    [Fact]
    public async Task DependencyFullFilePath()
    {
        using var sw = new StringWriter();
        var project = Path.Combine(TestUtil.SourceDirectory, "Sandbox", "SampleLibrary", "SampleLibrary.csproj");
        await new SourceExpanderCommand { Stdout = sw }.Dependency(project, fullFilePath: true, cancellationToken: TestContext.Current.CancellationToken);

        var obj = JsonSerializer.Deserialize<DependencyResult[]>(sw.ToString());
        obj.ShouldNotBeNull();
        var dic = obj!.ToDictionary(e => e.FileName);

        dic.ShouldContainKeyAndValue("ac-library-csharp>Graph/Dsu.cs",
            new("ac-library-csharp>Graph/Dsu.cs",
            ["ac-library-csharp>Internal/SimpleList.cs"],
            ["AtCoder.Dsu"]));

        dic.ShouldContainKeyAndValue(Path.Combine(TestUtil.SourceDirectory, "Sandbox", "SampleLibrary", "UnionFind.cs"),
            new(Path.Combine(TestUtil.SourceDirectory, "Sandbox", "SampleLibrary", "UnionFind.cs"),
            ["ac-library-csharp>Graph/Dsu.cs"],
            ["SampleLibrary.UnionFind"]));

        dic.ShouldContainKeyAndValue(Path.Combine(TestUtil.SourceDirectory, "Sandbox", "SampleLibrary", "UnsafeBlock.cs"),
            new(Path.Combine(TestUtil.SourceDirectory, "Sandbox", "SampleLibrary", "UnsafeBlock.cs"),
            [],
            ["SampleLibrary.UnsafeBlock"]));
    }
}
