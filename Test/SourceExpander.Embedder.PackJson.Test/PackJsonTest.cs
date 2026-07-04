using System.Text;

namespace SourceExpander;

public class PackJsonTest
{
    static readonly string ExtractedPackageDirectory = Path.Combine(TestUtil.PackageDirectory, "SampleLibraryJsonPack");

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
    public async Task MetadataDllWithEmbeddedJson(Target target)
    {
        var file = FileInfo(ExtractedPackageDirectory, "lib", target.TargetFramework, "SampleLibraryJsonPack.dll");
        await Assert.That(file).Exists();

        var metadata = TestUtil.GetSourceExpanderMetadata(file);

        await Assert.That(metadata).Count().IsEqualTo(1)
            .And.ContainsKeyWithValue("SourceExpander.EmbedderVersion", EmbedderVersion);
    }

    [Test]
    [MethodDataSource(nameof(TargetFrameworksWithoutEmbeddedJson))]
    public async Task BuildDirWithoutEmbeddedJson(Target target)
    {
        var file = FileInfo(ExtractedPackageDirectory, "lib", target.TargetFramework, "SampleLibraryJsonPack.dll");
        await Assert.That(file).Exists();

        var metadata = TestUtil.GetSourceExpanderMetadata(file);

        await Assert.That(metadata).Count().IsEqualTo(5)
            .And.ContainsKeyWithValue("SourceExpander.EmbedderVersion", EmbedderVersion)
            .And.ContainsKeyWithValue("SourceExpander.EmbeddedAllowUnsafe", "true")
            .And.ContainsKey("SourceExpander.EmbeddedLanguageVersion")
            .And.ContainsKey("SourceExpander.EmbeddedSourceCode")
            .And.ContainsKey("SourceExpander.EmbeddedNamespaces");
    }

    [Test]
    [MethodDataSource(nameof(TargetFrameworksWithEmbeddedJson))]
    public async Task BuildDirWithEmbeddedJson(Target target)
    {
        var directory = DirectoryInfo(ExtractedPackageDirectory, "buildTransitive", target.TargetFramework);
        await Assert.That(directory).Exists();
        var jsonFile = FileInfo(ExtractedPackageDirectory, "buildTransitive", target.TargetFramework, "SampleLibraryJsonPack_SourceExpander.Embedded.json");
        await Assert.That(jsonFile).Exists();

        var propsFile = FileInfo(ExtractedPackageDirectory, "buildTransitive", target.TargetFramework, "SampleLibraryJsonPack.props");
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

        var targetsFile = FileInfo(ExtractedPackageDirectory, "buildTransitive", target.TargetFramework, "SampleLibraryJsonPack.targets");
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
    public async Task MetadataDllWithoutEmbeddedJson(Target target)
    {
        var directory = DirectoryInfo(ExtractedPackageDirectory, "buildTransitive", target.TargetFramework);
        await Assert.That(directory).DoesNotExist();
    }
}
