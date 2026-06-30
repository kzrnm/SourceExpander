using System.IO;
using Microsoft.Build.Framework;

namespace SourceExpander;

public class ReadEmbeddedData : Microsoft.Build.Utilities.Task
{
    [Required]
    public string AssemblyPath { get; set; }
    [Output]
    public string OutputPath { get; set; }
    [Output]
    public string OutputName { get; set; }
    public override bool Execute()
    {
        OutputPath = AssemblyPath.Replace(".dll", "_SourceExpander.Embedded.json");
        OutputName = Path.GetFileName(OutputPath);
        try
        {
            var embeddedJson = ReadEmbeddedJson();

            if (embeddedJson == null)
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

    string ReadEmbeddedJson()
    {
        foreach (var pair in SourceExpanderMetadata.Load(AssemblyPath).Attributes)
        {
            if (pair.Key == "SourceExpander.EmbeddedDataJson")
            {
                return pair.Value;
            }
        }
        return null;
    }
}
