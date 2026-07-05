using System.Text;

namespace SourceExpander;

using static TestUtil;

public class PackJsonTest
{
    static FileInfo FileInfo(params ReadOnlySpan<string> paths) => new(Path.Combine(paths));
    static DirectoryInfo DirectoryInfo(params ReadOnlySpan<string> paths) => new(Path.Combine(paths));

    static string EmbedderVersion => typeof(PackJsonTest).Assembly.GetName().Version.ToString()!;

    public record Target(string TargetFramework, bool HasProps = true, bool HasTargets = true)
    {
        public override string ToString() => TargetFramework;
    }
    public static IEnumerable<Target> TargetFrameworksWithEmbeddedJson()
    {
        yield return new("net10.0");
        yield return new("net9.0", HasProps: false);
        yield return new("netstandard2.1", HasTargets: false);
    }
    public static IEnumerable<Target> TargetFrameworksWithoutEmbeddedJson()
    {
        yield return new("net8.0");
    }
    public static IEnumerable<string> ExtractedPackageDirectories() => TestUtil.ExtractedPackageDirectories;

    [Test]
    [MatrixDataSource]
    public async Task MetadataDllWithEmbeddedJson(
        [MatrixMethod<PackJsonTest>(nameof(ExtractedPackageDirectories))] string root,
        [MatrixMethod<PackJsonTest>(nameof(TargetFrameworksWithEmbeddedJson))] Target target)
    {
        var file = FileInfo(root, "lib", target.TargetFramework, "SampleLibraryJsonPack.dll");
        await Assert.That(file).Exists();

        var metadata = GetSourceExpanderMetadata(file);

        await Assert.That(metadata).Count().IsEqualTo(1)
            .And.ContainsKeyWithValue("SourceExpander.EmbedderVersion", EmbedderVersion);
    }

    [Test]
    [MatrixDataSource]
    public async Task BuildDirWithoutEmbeddedJson(
        [MatrixMethod<PackJsonTest>(nameof(ExtractedPackageDirectories))] string root,
        [MatrixMethod<PackJsonTest>(nameof(TargetFrameworksWithoutEmbeddedJson))] Target target)
    {
        var file = FileInfo(root, "lib", target.TargetFramework, "SampleLibraryJsonPack.dll");
        await Assert.That(file).Exists();

        var metadata = GetSourceExpanderMetadata(file);

        await Assert.That(metadata).Count().IsEqualTo(5)
            .And.ContainsKeyWithValue("SourceExpander.EmbedderVersion", EmbedderVersion)
            .And.ContainsKeyWithValue("SourceExpander.EmbeddedAllowUnsafe", "true")
            .And.ContainsKey("SourceExpander.EmbeddedLanguageVersion")
            .And.ContainsKey("SourceExpander.EmbeddedSourceCode")
            .And.ContainsKey("SourceExpander.EmbeddedNamespaces");
    }

    [Test]
    [MatrixDataSource]
    public async Task BuildDirWithEmbeddedJson(
        [MatrixMethod<PackJsonTest>(nameof(ExtractedPackageDirectories))] string root,
        [MatrixMethod<PackJsonTest>(nameof(TargetFrameworksWithEmbeddedJson))] Target target)
    {
        var directory = DirectoryInfo(root, "buildTransitive", target.TargetFramework);
        await Assert.That(directory).Exists();
        var jsonFile = FileInfo(root, "buildTransitive", target.TargetFramework, "SampleLibraryJsonPack_SourceExpander.Embedded.json");
        await Assert.That(jsonFile).Exists();

        var propsFile = FileInfo(root, "buildTransitive", target.TargetFramework, "SampleLibraryJsonPack.props");
        if (target.HasProps)
        {
            await Assert.That(propsFile).Exists();
            await Assert.That(File.ReadAllText(propsFile.FullName, new UTF8Encoding(false))).IsEqualTo(
                """
                <Project>
                  <PropertyGroup>
                    <SampleLibraryJsonPack_Source>$(MSBuildThisFileDirectory)SampleLibraryJsonPack_SourceExpander.Embedded.json</SampleLibraryJsonPack_Source>
                    <SampleLibraryJsonPack_Source_Visible>false</SampleLibraryJsonPack_Source_Visible>
                  </PropertyGroup>
                </Project>
                
                """);
        }
        else
        {
            await Assert.That(propsFile).DoesNotExist();
        }

        var targetsFile = FileInfo(root, "buildTransitive", target.TargetFramework, "SampleLibraryJsonPack.targets");
        if (target.HasTargets)
        {
            await Assert.That(targetsFile).Exists();
            await Assert.That(File.ReadAllText(targetsFile.FullName, new UTF8Encoding(false))).IsEqualTo(
                """
                <Project>
                  <ItemGroup Condition="'$(SourceExpander_Generator)'=='true' And Exists('$(SampleLibraryJsonPack_Source)')">
                    <AdditionalFiles LinkBase="Properties/SourceExpander.Embedded"
                      Include="$(SampleLibraryJsonPack_Source)"
                      Visible="$(SampleLibraryJsonPack_Source_Visible)" />
                  </ItemGroup>
                </Project>
                
                """);
        }
        else
        {
            await Assert.That(targetsFile).DoesNotExist();
        }
    }

    [Test]
    [MatrixDataSource]
    public async Task MetadataDllWithoutEmbeddedJson(
        [MatrixMethod<PackJsonTest>(nameof(ExtractedPackageDirectories))] string root,
        [MatrixMethod<PackJsonTest>(nameof(TargetFrameworksWithoutEmbeddedJson))] Target target)
    {
        var directory = DirectoryInfo(root, "buildTransitive", target.TargetFramework);
        await Assert.That(directory).DoesNotExist();
    }
}
