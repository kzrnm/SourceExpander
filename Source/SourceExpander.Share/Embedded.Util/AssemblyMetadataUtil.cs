using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace SourceExpander
{
    public static class AssemblyMetadataUtil
    {
        public static IEnumerable<(EmbeddedData Data, string? Display, ImmutableArray<(string Key, string ErrorMessage)> Errors)> GetEmbeddedSourceFiles(Compilation compilation)
        {
            foreach (var reference in compilation.References)
            {
                var symbol = compilation.GetAssemblyOrModuleSymbol(reference);
                if (symbol is null)
                    continue;


                var (embedded, errors) = EmbeddedData.Create(symbol.Name,
                    ImmutableDictionary.CreateRange(symbol.GetAttributes().Select(GetAttributeSourceCode).OfType<KeyValuePair<string, string>>())
                    );

                if (!embedded.IsEmpty || errors.Any())
                    yield return (embedded, reference.Display, errors);
            }
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
