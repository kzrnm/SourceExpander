using System.Text.Json;

namespace SourceExpander;

[NotInParallel(Initializer.CommandTests)]
public class CommandDependencyTests
{
    [Test]
    public async Task DependencySampleAppSkipAtcoder(CancellationToken cancellationToken)
    {
        using var sw = new StringWriter();
        var project = Path.Combine(TestUtil.TestProjectDirectory, "tools", "SampleAppSkipAtcoder.csproj");
        await new SourceExpanderCommand { Stdout = sw }.Dependency(project, cancellationToken: cancellationToken);

        var obj = JsonSerializer.Deserialize<DependencyResult[]>(sw.ToString());
        await obj.Should().NotBeNull();
        var dic = obj!.ToDictionary(e => e.FileName);

        await dic.ShouldContainKeyAndValue(Path.Combine(TestUtil.SourceDirectory, "Sandbox", "SampleApp", "Program.cs"),
            new(Path.Combine(TestUtil.SourceDirectory, "Sandbox", "SampleApp", "Program.cs"),
            [
            "SampleLibrary>nionFind.cs",
            ],
            []));

        await dic.ShouldContainKeyAndValue(Path.Combine(TestUtil.SourceDirectory, "Sandbox", "SampleApp", "Program2.cs"),
            new(Path.Combine(TestUtil.SourceDirectory, "Sandbox", "SampleApp", "Program2.cs"),
            ["SampleLibrary>nsafeBlock.cs"],
            []));

        await dic.ShouldContainKeyAndValue(Path.Combine(TestUtil.SourceDirectory, "Sandbox", "SampleApp", "Program3.cs"),
            new(Path.Combine(TestUtil.SourceDirectory, "Sandbox", "SampleApp", "Program3.cs"),
            ["Acl>IntOperator.cs"],
            []));

        await dic.Should().NotContainKey("ac-library-csharp>Graph/Dsu.cs");

        // `Un` is common prefix!
        await dic.ShouldContainKeyAndValue("SampleLibrary>nionFind.cs",
            new("SampleLibrary>nionFind.cs",
            ["ac-library-csharp>Graph/Dsu.cs"],
            ["SampleLibrary.UnionFind"]));

        // `Un` is common prefix!
        await dic.ShouldContainKeyAndValue("SampleLibrary>nsafeBlock.cs",
            new("SampleLibrary>nsafeBlock.cs",
            [],
            ["SampleLibrary.UnsafeBlock"]));
    }

    [Test]
    public async Task DependencySampleApp(CancellationToken cancellationToken)
    {
        using var sw = new StringWriter();
        var project = Path.Combine(TestUtil.SourceDirectory, "Sandbox", "SampleApp", "SampleApp.csproj");
        await new SourceExpanderCommand { Stdout = sw }.Dependency(project, cancellationToken: cancellationToken);

        var obj = JsonSerializer.Deserialize<DependencyResult[]>(sw.ToString());
        await obj.Should().NotBeNull();
        var dic = obj!.ToDictionary(e => e.FileName);


        await dic.ShouldContainKeyAndValue(Path.Combine(TestUtil.SourceDirectory, "Sandbox", "SampleApp", "Program.cs"),
            new(Path.Combine(TestUtil.SourceDirectory, "Sandbox", "SampleApp", "Program.cs"),
            [
            "ac-library-csharp>Graph/Dsu.cs",
            "ac-library-csharp>Internal/SimpleList.cs",
            "SampleLibrary>nionFind.cs",
            ],
            []));

        await dic.ShouldContainKeyAndValue(Path.Combine(TestUtil.SourceDirectory, "Sandbox", "SampleApp", "Program2.cs"),
            new(Path.Combine(TestUtil.SourceDirectory, "Sandbox", "SampleApp", "Program2.cs"),
            ["SampleLibrary>nsafeBlock.cs"],
            []));

        await dic.ShouldContainKeyAndValue("ac-library-csharp>Graph/Dsu.cs",
            new("ac-library-csharp>Graph/Dsu.cs",
            ["ac-library-csharp>Internal/SimpleList.cs"],
            ["AtCoder.Dsu"]));

        // `Un` is common prefix!
        await dic.ShouldContainKeyAndValue("SampleLibrary>nionFind.cs",
            new("SampleLibrary>nionFind.cs",
            ["ac-library-csharp>Graph/Dsu.cs"],
            ["SampleLibrary.UnionFind"]));

        // `Un` is common prefix!
        await dic.ShouldContainKeyAndValue("SampleLibrary>nsafeBlock.cs",
            new("SampleLibrary>nsafeBlock.cs",
            [],
            ["SampleLibrary.UnsafeBlock"]));
    }

    [Test]
    public async Task DependencySampleLibrary(CancellationToken cancellationToken)
    {
        using var sw = new StringWriter();
        var project = Path.Combine(TestUtil.SourceDirectory, "Sandbox", "SampleLibrary", "SampleLibrary.csproj");
        await new SourceExpanderCommand { Stdout = sw }.Dependency(project, cancellationToken: cancellationToken);

        var obj = JsonSerializer.Deserialize<DependencyResult[]>(sw.ToString());
        await obj.Should().NotBeNull();
        var dic = obj!.ToDictionary(e => e.FileName);

        await dic.ShouldContainKeyAndValue("ac-library-csharp>Graph/Dsu.cs",
            new("ac-library-csharp>Graph/Dsu.cs",
            ["ac-library-csharp>Internal/SimpleList.cs"],
            ["AtCoder.Dsu"]));

        // `Un` is common prefix!
        await dic.ShouldContainKeyAndValue("SampleLibrary>nionFind.cs",
            new("SampleLibrary>nionFind.cs",
            ["ac-library-csharp>Graph/Dsu.cs"],
            ["SampleLibrary.UnionFind"]));

        // `Un` is common prefix!
        await dic.ShouldContainKeyAndValue("SampleLibrary>nsafeBlock.cs",
            new("SampleLibrary>nsafeBlock.cs",
            [],
            ["SampleLibrary.UnsafeBlock"]));
    }


    [Test]
    public async Task DependencyFullFilePath(CancellationToken cancellationToken)
    {
        using var sw = new StringWriter();
        var project = Path.Combine(TestUtil.SourceDirectory, "Sandbox", "SampleLibrary", "SampleLibrary.csproj");
        await new SourceExpanderCommand { Stdout = sw }.Dependency(project, fullFilePath: true, cancellationToken: cancellationToken);

        var obj = JsonSerializer.Deserialize<DependencyResult[]>(sw.ToString());
        await obj.Should().NotBeNull();
        var dic = obj!.ToDictionary(e => e.FileName);

        await dic.ShouldContainKeyAndValue("ac-library-csharp>Graph/Dsu.cs",
            new("ac-library-csharp>Graph/Dsu.cs",
            ["ac-library-csharp>Internal/SimpleList.cs"],
            ["AtCoder.Dsu"]));

        await dic.ShouldContainKeyAndValue(Path.Combine(TestUtil.SourceDirectory, "Sandbox", "SampleLibrary", "UnionFind.cs"),
            new(Path.Combine(TestUtil.SourceDirectory, "Sandbox", "SampleLibrary", "UnionFind.cs"),
            ["ac-library-csharp>Graph/Dsu.cs"],
            ["SampleLibrary.UnionFind"]));

        await dic.ShouldContainKeyAndValue(Path.Combine(TestUtil.SourceDirectory, "Sandbox", "SampleLibrary", "UnsafeBlock.cs"),
            new(Path.Combine(TestUtil.SourceDirectory, "Sandbox", "SampleLibrary", "UnsafeBlock.cs"),
            [],
            ["SampleLibrary.UnsafeBlock"]));
    }
}
