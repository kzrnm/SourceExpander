namespace SourceExpander;

[NotInParallel(Initializer.CommandTests)]
public class CommandLibraryListTests
{
    [Test]
    public async Task LibraryListSampleAppSkipAtcoder(CancellationToken cancellationToken)
    {
        using var sw = new StringWriter();
        var project = Path.Combine(TestUtil.SourceDirectory, "Sandbox", "SampleAppSkipAtcoder", "SampleAppSkipAtcoder.csproj");
        await new SourceExpanderCommand { Stdout = sw }.LibraryList(project, cancellationToken: cancellationToken);

        await sw.ToString().ReplaceLineEndings().Should().BeEqualTo(
$"""
ac-library-csharp-override,1.0.0
BigInteger,9.1.1.100
SampleLibrary,{AssemblyUtil.AssemblyVersion}

""".ReplaceLineEndings());
    }

    [Test]
    public async Task LibraryListSampleApp(CancellationToken cancellationToken)
    {
        using var sw = new StringWriter();
        var project = Path.Combine(TestUtil.SourceDirectory, "Sandbox", "SampleApp", "SampleApp.csproj");
        await new SourceExpanderCommand { Stdout = sw }.LibraryList(project, cancellationToken: cancellationToken);

        await sw.ToString().ReplaceLineEndings().Should().BeEqualTo(
$"""
ac-library-csharp,7.0.0.100
ac-library-csharp-override,1.0.0
BigInteger,9.1.1.100
SampleLibrary,{AssemblyUtil.AssemblyVersion}

""".ReplaceLineEndings());
    }

    [Test]
    public async Task LibraryListSampleLibrary(CancellationToken cancellationToken)
    {
        using var sw = new StringWriter();
        var project = Path.Combine(TestUtil.SourceDirectory, "Sandbox", "SampleLibrary", "SampleLibrary.csproj");
        await new SourceExpanderCommand { Stdout = sw }.LibraryList(project, cancellationToken: cancellationToken);

        await sw.ToString().ReplaceLineEndings().Should().BeEqualTo(
$"""
ac-library-csharp,7.0.0.100
BigInteger,9.1.1.100
SampleLibrary,{AssemblyUtil.AssemblyVersion}

""".ReplaceLineEndings());
    }
}
