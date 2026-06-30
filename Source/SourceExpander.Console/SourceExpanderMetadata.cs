using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace SourceExpander;

internal class SourceExpanderMetadata
{
    private SourceExpanderMetadata(string assemblyName, ImmutableArray<KeyValuePair<string, string>> attributesData)
    {
        AssemblyName = assemblyName;
        Attributes = attributesData;
    }
    public string AssemblyName { get; }
    public ImmutableArray<KeyValuePair<string, string>> Attributes { get; }
    public static SourceExpanderMetadata Load(string assemblyPath)
    {
        using var stream = File.OpenRead(assemblyPath);
        using var peReader = new PEReader(stream);

        var metadataReader = peReader.GetMetadataReader();
        string assemblyName = metadataReader.GetString(metadataReader.GetAssemblyDefinition().Name);
        return new(assemblyName, EnumerateSourceExpanderMetadata(metadataReader));
    }

    static ImmutableArray<KeyValuePair<string, string>> EnumerateSourceExpanderMetadata(MetadataReader metadataReader)
    {
        var metadata = ImmutableArray.CreateBuilder<KeyValuePair<string, string>>();
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
                        metadata.Add(new KeyValuePair<string, string>(key, value));
                    }
                }
            }
        }
        return metadata.ToImmutable();
    }
}
