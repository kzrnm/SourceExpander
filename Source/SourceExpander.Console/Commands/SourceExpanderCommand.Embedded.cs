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
        var metadata = SourceExpanderMetadata.Load(target);
        return EmbeddedData.LoadFromMetadata(metadata.AssemblyName, metadata.Attributes).Data;
    }
}
