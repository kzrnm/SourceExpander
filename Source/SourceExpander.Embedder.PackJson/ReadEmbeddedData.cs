using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using Microsoft.Build.Framework;

namespace SourceExpander;

public class ReadEmbeddedData : Microsoft.Build.Utilities.Task
{
    const string EmbeddedDataJsonKey = "SourceExpander.EmbeddedDataJson";
    const string EmbedderVersionKey = "SourceExpander.EmbedderVersion";

    [Required]
    public string AssemblyPath { get; set; }
    [Output]
    public string OutputPath { get; set; }
    [Output]
    public string OutputName { get; set; }
    [Output]
    public string EmbedderVersion { get; set; }
    public override bool Execute()
    {
        OutputPath = AssemblyPath.Replace(".dll", "_SourceExpander.Embedded.json");
        OutputName = Path.GetFileName(OutputPath);
        try
        {
            string embeddedJson = null;

            foreach (var pair in SourceExpanderMetadata.Load(AssemblyPath).Attributes)
            {
                switch (pair.Key)
                {
                    case EmbeddedDataJsonKey:
                        embeddedJson = pair.Value;
                        break;
                    case EmbedderVersionKey:
                        EmbedderVersion = pair.Value;
                        break;
                }
            }

            if (embeddedJson is null)
            {
                Log.LogError("No embedded data found in assembly '{0}'. "
                    + """The assembly must contain AssemblyMetadataAttribute("SourceExpander.EmbeddedDataJson", json). """
                    + "SourceExpander.Embedder must be enabled and SourceExpander_Embedder_EmbeddingType must be 'SingleMetadataJson'.", AssemblyPath);
                return false;
            }

            if (EmbedderVersion is null)
            {
                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(embeddedJson)))
                {
                    var serializer = new DataContractJsonSerializer(typeof(MetadataJsonForVersion));
                    EmbedderVersion = (serializer.ReadObject(stream) as MetadataJsonForVersion)?.EmbedderVersion;
                }
                if (EmbedderVersion is null)
                {
                    Log.LogWarning("No embedder version found in embedded data of assembly '{0}'. "
                        + """The embedded data must contain "SourceExpander.EmbedderVersion": "version". """, AssemblyPath);
                }
            }

            Log.LogMessage(MessageImportance.High, "Writing embedded data of '{0}' to '{1}'", AssemblyPath, OutputPath);
            File.WriteAllText(OutputPath, embeddedJson);
            return true;
        }
        catch (System.Exception e)
        {
            Log.LogErrorFromException(e);
            return false;
        }
    }

    [DataContract]
    class MetadataJsonForVersion
    {
        [DataMember(Name = EmbedderVersionKey)]
        public string EmbedderVersion { get; set; }
    }
}
