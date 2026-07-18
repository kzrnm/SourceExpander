using System.IO;
using Microsoft.Build.Framework;

namespace SourceExpander;

public class ReadEmbeddedData : Microsoft.Build.Utilities.Task
{
    const string EmbeddedDataJsonKey = "SourceExpander.EmbeddedDataJson";

    [Required]
    public string AssemblyPath { get; set; }
    [Required]
    public string OutputPath { get; set; }
    public override bool Execute()
    {
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
                }
            }

            if (embeddedJson is null)
            {
                Log.LogError("No embedded data found in assembly '{0}'. "
                    + """The assembly must contain AssemblyMetadataAttribute("SourceExpander.EmbeddedDataJson", json). """
                    + "SourceExpander.Embedder must be enabled and SourceExpander_Embedder_EmbeddingType must be 'SingleMetadataJson'.", AssemblyPath);
                return false;
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
}
