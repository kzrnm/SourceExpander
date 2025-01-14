using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace SourceExpander;

public partial class CommandTests
{
    [Fact]
    public async Task LibraryListSampleAppSkipAtcoder()
    {
        using var sw = new StringWriter();
        var project = Path.Combine(TestUtil.TestProjectDirectory, "tools", "SampleAppSkipAtcoder.csproj");
        await new SourceExpanderCommand { Stdout = sw }.LibraryList(project, cancellationToken: TestContext.Current.CancellationToken);

        sw.ToString().ReplaceLineEndings().Should().Be(
$"""
SampleLibrary,{AssemblyUtil.AssemblyVersion}

""".ReplaceLineEndings());
    }

    [Fact]
    public async Task LibraryListSampleApp()
    {
        using var sw = new StringWriter();
        var project = Path.Combine(TestUtil.SourceDirectory, "Sandbox", "SampleApp", "SampleApp.csproj");
        await new SourceExpanderCommand { Stdout = sw }.LibraryList(project, cancellationToken: TestContext.Current.CancellationToken);

        sw.ToString().ReplaceLineEndings().Should().Be(
$"""
ac-library-csharp,7.0.0.100
SampleLibrary,{AssemblyUtil.AssemblyVersion}

""".ReplaceLineEndings());
    }

    [Fact]
    public async Task LibraryListSampleLibrary()
    {
        using var sw = new StringWriter();
        var project = Path.Combine(TestUtil.SourceDirectory, "Sandbox", "SampleLibrary", "SampleLibrary.csproj");
        await new SourceExpanderCommand { Stdout = sw }.LibraryList(project, cancellationToken: TestContext.Current.CancellationToken);

        sw.ToString().ReplaceLineEndings().Should().Be(
$"""
SampleLibrary,{AssemblyUtil.AssemblyVersion}
ac-library-csharp,7.0.0.100

""".ReplaceLineEndings());
    }
}
