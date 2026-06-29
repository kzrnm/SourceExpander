using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Mono.Cecil;

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
        using var assembly = AssemblyDefinition.ReadAssembly(target);
        var metadata = new List<KeyValuePair<string, string>>();
        int to = 0;
        for (int i = 0; i < assembly.CustomAttributes.Count; i++)
        {
            var a = assembly.CustomAttributes[i];
            if (TryParseSourceExpanderMetadata(a, out var pair))
            {
                metadata.Add(pair);
            }
            else
            {
                assembly.CustomAttributes[to++] = a;
            }
        }
        while (assembly.CustomAttributes.Count >= to)
        {
            assembly.CustomAttributes.RemoveAt(assembly.CustomAttributes.Count - 1);
        }
        var (embeddedData, _) = EmbeddedData.LoadFromMetadata(assembly.Name.Name, metadata);

        Output.WriteLine(JsonUtil.ToJson(embeddedData));
    }

    static bool TryParseSourceExpanderMetadata(CustomAttribute attribute, out KeyValuePair<string, string> result)
    {
        if (attribute is not { AttributeType.FullName: "System.Reflection.AssemblyMetadataAttribute", ConstructorArguments.Count: 2 })
            goto Failure;

        var keyArg = attribute.ConstructorArguments[0].Value;
        var valArg = attribute.ConstructorArguments[1].Value;

        if (keyArg is string key && key.StartsWith("SourceExpander.") && valArg is string val)
        {
            result = KeyValuePair.Create(key, val);
            return true;
        }
    Failure:
        result = default;
        return false;
    }
}
