using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;

namespace SourceExpander;

static class TestUtil
{
    static string ThisFileDir([CallerFilePath] string path = "") => Path.GetDirectoryName(path)!;
    static readonly string TestProjectDirectory = ThisFileDir();
    static readonly string SandboxDirectory = Path.GetFullPath(Path.Combine(TestProjectDirectory, "..", "..", "Source", "Sandbox"));
    static readonly string SampleLibraryProjectDirectory = Path.Combine(SandboxDirectory, "SampleLibraryJsonPack");
    public static readonly string PackageDirectory = Path.Combine(Path.GetDirectoryName(typeof(TestUtil).Assembly.Location), "SampleLibraryJsonPack");
    public static readonly string SampleLibraryProject = Path.Combine(SampleLibraryProjectDirectory, "SampleLibraryJsonPack.csproj");

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
