using System.Collections.Generic;
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

                foreach (var attributeData in symbol.GetAttributes())
                {
                    var infos = GetAttributeSourceCode(attributeData);
                    if (infos != null)
                        foreach (var info in infos)
                            result.Add(info);
                }
            }
            return result.ToArray();
        }

        static List<SourceFileInfo>? GetAttributeSourceCode(AttributeData attr)
        {
            if (attr.AttributeClass?.ToString() != "System.Reflection.AssemblyMetadataAttribute")
                return null;
            var args = attr.ConstructorArguments;
            if (!(args.Length == 2 && args[0].Value is string key && args[1].Value is string val))
                return null;
            return SourceFileInfoUtil.GetAttributeSourceFileInfos(new KeyValuePair<string, string>(key, val));
        }
    }
}
