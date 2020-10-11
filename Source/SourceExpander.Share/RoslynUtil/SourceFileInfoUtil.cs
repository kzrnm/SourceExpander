using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using Microsoft.CodeAnalysis;

namespace SourceExpander
{
    internal static class SourceFileInfoUtil
    {
        public static SourceFileInfo[] GetEmbeddedSourceFiles(Compilation compilation)
        {
            var result = new List<SourceFileInfo>();
            foreach (var reference in compilation.References)
            {
                var symbol = compilation.GetAssemblyOrModuleSymbol(reference);
                if (symbol is null) continue;
                foreach (var info in symbol.GetAttributes().Select(GetAttributeSourceCode).OfType<string>().SelectMany(ParseEmbeddedJson))
                {
                    result.Add(info);
                }
            }
            return result.ToArray();
        }
        static List<SourceFileInfo> ParseEmbeddedJson(string json)
        {
            using var ms = new MemoryStream(new UTF8Encoding(false).GetBytes(json));
            var serializer = new DataContractJsonSerializer(typeof(List<SourceFileInfo>));
            return (List<SourceFileInfo>)serializer.ReadObject(ms);

        }
        static string? GetAttributeSourceCode(AttributeData attr)
        {
            if (attr.AttributeClass?.ToString() != "System.Reflection.AssemblyMetadataAttribute")
                return null;
            var args = attr.ConstructorArguments;
            if (!(args.Length == 2 && args[0].Value is string key && args[1].Value is string val)) return null;
            if (key != "SourceExpander.EmbeddedSourceCode") return null;
            return val;
        }

    }
}
