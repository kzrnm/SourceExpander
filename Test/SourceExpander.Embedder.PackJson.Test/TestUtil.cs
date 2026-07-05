using System.Collections.Immutable;
using System.Diagnostics;
using System.IO.Compression;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using TUnit.Core.Logging;

namespace SourceExpander;

abstract class TestUtil
{
    [Before(Assembly)]
    public static async Task BuildProject(CancellationToken cancellationToken)
    {
        if (Directory.Exists(PackageDirectory))
        {
            foreach (var pkg in Directory.EnumerateFiles(PackageDirectory, "SampleLibraryJsonPack.*.nupkg"))
                File.Delete(pkg);

            foreach (var pkg in Directory.EnumerateDirectories(PackageDirectory, "SampleLibraryJsonPack"))
                Directory.Delete(pkg, true);
        }

        foreach (var dir in ExtractedPackageDirectories)
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                ArgumentList =
                {
                    "pack",
                    "SampleLibraryJsonPack.csproj",
                    "-c", "Release",
                    "-o", PackageDirectory,
                },
                StandardOutputEncoding = System.Text.Encoding.UTF8,
                StandardErrorEncoding = System.Text.Encoding.UTF8,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = SampleLibraryProjectDirectory,
                UseShellExecute = false,
                CreateNoWindow = true,
                EnvironmentVariables = { ["DOTNET_CLI_TELEMETRY_OPTOUT"] = "1", ["DOTNET_SKIP_FIRST_TIME_EXPERIENCE"] = "1" },
            };

            if (dir.EndsWith("PackNotBuild"))
                processStartInfo.ArgumentList.Add("--no-build");

            using var process = Process.Start(processStartInfo);
            await process.WaitForExitAsync(cancellationToken);

            var logger = TestContext.Current!.GetDefaultLogger();

            logger.LogInformation($"Process exited with code {process.ExitCode}");
            logger.LogInformation("---stdout---\n" + process.StandardOutput.ReadToEnd());
            logger.LogInformation("---stderr---\n" + process.StandardError.ReadToEnd());

            var nupkgFile = Directory.EnumerateFiles(PackageDirectory, "SampleLibraryJsonPack.*.nupkg").Single();
            await ZipFile.ExtractToDirectoryAsync(nupkgFile, dir, true, cancellationToken);
            File.Delete(nupkgFile);
        }
    }

    static string ThisFileDir([CallerFilePath] string path = "") => Path.GetDirectoryName(path)!;
    static readonly string TestProjectDirectory = ThisFileDir();
    static readonly string PackageDirectory = Path.Combine(Path.GetDirectoryName(typeof(TestUtil).Assembly.Location), "publish");
    static readonly string SandboxDirectory = Path.GetFullPath(Path.Combine(TestProjectDirectory, "..", "..", "Source", "Sandbox"));
    static readonly string SampleLibraryProjectDirectory = Path.Combine(SandboxDirectory, "SampleLibraryJsonPack");
    public static readonly string SampleLibraryProject = Path.Combine(SampleLibraryProjectDirectory, "SampleLibraryJsonPack.csproj");
    public static readonly ImmutableArray<string> ExtractedPackageDirectories = [
        Path.Combine(PackageDirectory, "PackAfterBuild"),
        Path.Combine(PackageDirectory, "PackNotBuild"),
    ];

    public static Dictionary<string, string> GetSourceExpanderMetadata(FileInfo file)
    {
        using var stream = File.OpenRead(file.FullName);
        using var peReader = new PEReader(stream);

        var actual = new Dictionary<string, string>();
        var metadataReader = peReader.GetMetadataReader();
        foreach (var attrHandle in metadataReader.GetAssemblyDefinition().GetCustomAttributes())
        {
            var attribute = metadataReader.GetCustomAttribute(attrHandle);

            // 属性のコンストラクタ（型情報）を取得
            if (attribute.Constructor.Kind == HandleKind.MemberReference)
            {
                var memberRef = metadataReader.GetMemberReference((MemberReferenceHandle)attribute.Constructor);
                var typeRef = metadataReader.GetTypeReference((TypeReferenceHandle)memberRef.Parent);

                if (metadataReader.GetString(typeRef.Name) is "AssemblyMetadataAttribute")
                {
                    var blob = metadataReader.GetBlobReader(attribute.Value);
                    if (blob.ReadUInt16() == 1
                        && blob.ReadSerializedString() is string key
                        && key.StartsWith("SourceExpander.")
                        && blob.ReadSerializedString() is string value)
                    {
                        actual[key] = value;
                    }
                }
            }
        }
        return actual;
    }
}
