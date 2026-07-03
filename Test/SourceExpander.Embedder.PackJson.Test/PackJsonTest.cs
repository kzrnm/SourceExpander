using System.Diagnostics;
using System.IO.Compression;
using System.Text;

namespace SourceExpander;

public class PackJsonTest
{
    [Before(Class)]
    public static async Task BuildProject(CancellationToken cancellationToken)
    {
        if (Directory.Exists(TestUtil.PackageDirectory))
        {
            foreach (var pkg in Directory.EnumerateFiles(TestUtil.PackageDirectory, "SampleLibrary.*.nupkg"))
                File.Delete(pkg);

            foreach (var pkg in Directory.EnumerateDirectories(TestUtil.PackageDirectory, "SampleLibrary"))
                Directory.Delete(pkg, true);

            File.Delete(Path.Combine(TestUtil.PackageDirectory, "SampleLibrary.1.0.0.nupkg"));
        }
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            ArgumentList =
            {
                "pack",
                "SampleLibrary.csproj",
                "-c", "Release",
                "-o", TestUtil.PackageDirectory,
                "-p:PackageTesting=true",
            },
            StandardOutputEncoding = System.Text.Encoding.UTF8,
            StandardErrorEncoding = System.Text.Encoding.UTF8,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = TestUtil.SampleLibraryProjectDirectory,
            UseShellExecute = false,
            CreateNoWindow = true,
            EnvironmentVariables = { ["DOTNET_CLI_TELEMETRY_OPTOUT"] = "1", ["DOTNET_SKIP_FIRST_TIME_EXPERIENCE"] = "1" },
        };

        using var process = Process.Start(processStartInfo);
        await process.WaitForExitAsync(cancellationToken);

        var nupkgFile = Directory.EnumerateFiles(TestUtil.PackageDirectory, "SampleLibrary.*.nupkg").Single();
        await ZipFile.ExtractToDirectoryAsync(nupkgFile, Path.Combine(TestUtil.PackageDirectory, "SampleLibrary"), true, cancellationToken);
    }

    static readonly string ExtractedPackageDirectory = Path.Combine(TestUtil.PackageDirectory, "SampleLibrary");

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
        var file = FileInfo(ExtractedPackageDirectory, "lib", target.TargetFramework, "SampleLibrary.dll");
        await Assert.That(file).Exists();

        var metadata = TestUtil.GetSourceExpanderMetadata(file);

        await Assert.That(metadata).Count().IsEqualTo(1)
            .And.ContainsKeyWithValue("SourceExpander.EmbedderVersion", EmbedderVersion);
    }

    [Test]
    [MethodDataSource(nameof(TargetFrameworksWithoutEmbeddedJson))]
    public async Task BuildDirWithoutEmbeddedJson(Target target)
    {
        var file = FileInfo(ExtractedPackageDirectory, "lib", target.TargetFramework, "SampleLibrary.dll");
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
        var jsonFile = FileInfo(ExtractedPackageDirectory, "buildTransitive", target.TargetFramework, "SampleLibrary_SourceExpander.Embedded.json");
        await Assert.That(jsonFile).Exists();

        var propsFile = FileInfo(ExtractedPackageDirectory, "buildTransitive", target.TargetFramework, "SampleLibrary.props");
        if (target.HasProps)
        {
            await Assert.That(propsFile).Exists();
            await Assert.That(File.ReadAllText(propsFile.FullName, new UTF8Encoding(false))).IsEqualTo(
                """
                <Project>
                  <PropertyGroup>
                    <SampleLibrary_Source>$(MSBuildThisFileDirectory)SampleLibrary_SourceExpander.Embedded.json</SampleLibrary_Source>
                    <SampleLibrary_Source_Visible>false</SampleLibrary_Source_Visible>
                  </PropertyGroup>
                </Project>
                
                """);
        }
        else
        {
            await Assert.That(propsFile).DoesNotExist();
        }

        var targetsFile = FileInfo(ExtractedPackageDirectory, "buildTransitive", target.TargetFramework, "SampleLibrary.targets");
        if (target.HasTargets)
        {
            await Assert.That(targetsFile).Exists();
            await Assert.That(File.ReadAllText(targetsFile.FullName, new UTF8Encoding(false))).IsEqualTo(
                """
                <Project>
                  <ItemGroup Condition="'$(SourceExpander_Generator)'=='true' And Exists('$(SampleLibrary_Source)')">
                    <AdditionalFiles LinkBase="Properties/SourceExpander.Embedded"
                      Include="$(SampleLibrary_Source)"
                      Visible="$(SampleLibrary_Source_Visible)" />
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
