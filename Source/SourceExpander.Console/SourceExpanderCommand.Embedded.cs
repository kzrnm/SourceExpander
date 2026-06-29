using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Threading.Tasks;

namespace SourceExpander;


partial struct SourceExpanderCommand
{
    /// <summary>
    /// Show the embedded data.
    /// </summary>
    /// <param name="target">Target DLL file.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    [Command("embedded")]
    public async Task Embedded([Argument] string target)
    {
        var data = ReadEmbeddedData(target);
        Output.WriteLine(JsonUtil.ToJson(data));
    }


    static EmbeddedData ReadEmbeddedData(string target)
    {
        using var stream = File.OpenRead(target);
        using var peReader = new PEReader(stream);

        var metadataReader = peReader.GetMetadataReader();
        string assemblyName = metadataReader.GetString(metadataReader.GetAssemblyDefinition().Name);
        return EmbeddedData.LoadFromMetadata(assemblyName, EnumerateSourceExpanderMetadata(metadataReader)).Data;
    }

    static List<KeyValuePair<string, string>> EnumerateSourceExpanderMetadata(MetadataReader metadataReader)
    {
        var metadata = new List<KeyValuePair<string, string>>();
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
                        metadata.Add(KeyValuePair.Create(key, value));
                    }
                }
            }
        }
        return metadata;
    }
}
