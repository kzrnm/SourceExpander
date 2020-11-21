using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace SourceExpander
{
    public static class AssemblyMetadataUtil
    {
        public static SourceFileInfo[] GetEmbeddedSourceFiles(Compilation compilation)
        {
            var result = new List<SourceFileInfo>();
            foreach (var reference in compilation.References)
            {
                var symbol = compilation.GetAssemblyOrModuleSymbol(reference);
                if (symbol is null)
                    continue;


                var name = symbol.Name;
                foreach (var source in EmbeddedData.Create(symbol.Name, symbol.GetAttributes()
                    .Select(GetAttributeSourceCode)
                    .OfType<KeyValuePair<string, string>>()).Sources)
                {
                    result.Add(source);
                }
            }
            return result.ToArray();
        }

        static KeyValuePair<string, string>? GetAttributeSourceCode(AttributeData attr)
        {
            if (attr.AttributeClass?.ToString() != "System.Reflection.AssemblyMetadataAttribute")
                return null;
            var args = attr.ConstructorArguments;
            if (!(args.Length == 2 && args[0].Value is string key && args[1].Value is string val))
                return null;
            return new KeyValuePair<string, string>(key, val);
        }
    }
}
