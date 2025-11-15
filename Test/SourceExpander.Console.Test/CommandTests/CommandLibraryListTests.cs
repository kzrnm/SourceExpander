using System.IO;
using System.Threading.Tasks;

namespace SourceExpander;

[NotInParallel(Initializer.CommandTests)]
public class CommandLibraryListTests
{
    [Test]
    public async Task LibraryListSampleAppSkipAtcoder()
    {
        using var sw = new StringWriter();
        var project = Path.Combine(TestUtil.TestProjectDirectory, "tools", "SampleAppSkipAtcoder.csproj");
        await new SourceExpanderCommand { Stdout = sw }.LibraryList(project, cancellationToken: TestContext.Current!.Execution.CancellationToken);

        sw.ToString().ReplaceLineEndings().ShouldBe(
$"""
SampleLibrary,{AssemblyUtil.AssemblyVersion}

""".ReplaceLineEndings());
    }

    [Test]
    public async Task LibraryListSampleApp()
    {
        using var sw = new StringWriter();
        var project = Path.Combine(TestUtil.SourceDirectory, "Sandbox", "SampleApp", "SampleApp.csproj");
        await new SourceExpanderCommand { Stdout = sw }.LibraryList(project, cancellationToken: TestContext.Current!.Execution.CancellationToken);

        sw.ToString().ReplaceLineEndings().ShouldBe(
$"""
ac-library-csharp,7.0.0.100
SampleLibrary,{AssemblyUtil.AssemblyVersion}

""".ReplaceLineEndings());
    }

    [Test]
    public async Task LibraryListSampleLibrary()
    {
        using var sw = new StringWriter();
        var project = Path.Combine(TestUtil.SourceDirectory, "Sandbox", "SampleLibrary", "SampleLibrary.csproj");
        await new SourceExpanderCommand { Stdout = sw }.LibraryList(project, cancellationToken: TestContext.Current!.Execution.CancellationToken);

        sw.ToString().ReplaceLineEndings().ShouldBe(
$"""
SampleLibrary,{AssemblyUtil.AssemblyVersion}
ac-library-csharp,7.0.0.100

""".ReplaceLineEndings());
    }
}
