using System.Text;


namespace SourceExpander;

using static TestUtil;

public class Dummy
{
    [Test]
    public void DummyTest() { }
}

[Skip("wip")]
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

    [Test]
    [MethodDataSource(nameof(TargetFrameworksWithEmbeddedJson))]
    public async Task MetadataDllWithEmbeddedJson(
        Target target)
    {
        var file = FileInfo(PackageDirectory, "lib", target.TargetFramework, "SampleLibraryJsonPack.dll");
        await Assert.That(file).Exists();

        var metadata = GetSourceExpanderMetadata(file);

        await Assert.That(metadata).Count().IsEqualTo(1)
            .And.ContainsKeyWithValue("SourceExpander.EmbedderVersion", EmbedderVersion);
    }

    [Test]
    [MethodDataSource(nameof(TargetFrameworksWithoutEmbeddedJson))]
    public async Task BuildDirWithoutEmbeddedJson(
        Target target)
    {
        var file = FileInfo(PackageDirectory, "lib", target.TargetFramework, "SampleLibraryJsonPack.dll");
        await Assert.That(file).Exists();

        var metadata = GetSourceExpanderMetadata(file);

        await Assert.That(metadata).Count().IsEqualTo(2)
            .And.ContainsKeyWithValue("SourceExpander.EmbedderVersion", EmbedderVersion)
            .And.ContainsKey("SourceExpander.EmbeddedDataJson");
    }

    [Test]
    [MethodDataSource(nameof(TargetFrameworksWithEmbeddedJson))]
    public async Task BuildDirWithEmbeddedJson(
        Target target)
    {
        var directory = DirectoryInfo(PackageDirectory, "buildTransitive", target.TargetFramework);
        await Assert.That(directory).Exists();
        var jsonFile = FileInfo(PackageDirectory, "buildTransitive", target.TargetFramework, "SampleLibraryJsonPack_SourceExpander.Embedded.json");
        await Assert.That(jsonFile).Exists();

        var propsFile = FileInfo(PackageDirectory, "buildTransitive", target.TargetFramework, "SampleLibraryJsonPack.props");
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

        var targetsFile = FileInfo(PackageDirectory, "buildTransitive", target.TargetFramework, "SampleLibraryJsonPack.targets");
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
    [MethodDataSource(nameof(TargetFrameworksWithoutEmbeddedJson))]
    public async Task MetadataDllWithoutEmbeddedJson(
        Target target)
    {
        var directory = DirectoryInfo(PackageDirectory, "buildTransitive", target.TargetFramework);
        await Assert.That(directory).DoesNotExist();
    }
}
