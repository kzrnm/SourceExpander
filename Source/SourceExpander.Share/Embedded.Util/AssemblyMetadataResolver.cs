using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace SourceExpander
{
    public class AssemblyMetadataResolver
    {
        private readonly Compilation compilation;
        private INamedTypeSymbol? _System_Reflection_AssemblyMetadataAttribute;
        private INamedTypeSymbol System_Reflection_AssemblyMetadataAttribute
            => _System_Reflection_AssemblyMetadataAttribute
            ??= compilation.GetTypeByMetadataName("System.Reflection.AssemblyMetadataAttribute")
            ?? throw new Exception("System.Reflection.AssemblyMetadataAttribute is not found");
        public AssemblyMetadataResolver(Compilation compilation)
        {
            this.compilation = compilation;
        }

        public IEnumerable<(EmbeddedData Data, string? Display, ImmutableArray<(string Key, string ErrorMessage)> Errors)>
            GetEmbeddedSourceFiles()
        {
            foreach (var reference in compilation.References)
            {
                var symbol = compilation.GetAssemblyOrModuleSymbol(reference);
                if (symbol is null)
                    continue;

                var (embedded, errors) = EmbeddedData.Create(symbol.Name,
                    symbol.GetAttributes()
                        .Select(GetAttributeSourceCode)
                        .OfType<KeyValuePair<string, string>>());

                yield return (embedded, reference.Display, errors);
            }
        }
        KeyValuePair<string, string>? GetAttributeSourceCode(AttributeData attr)
        {
            if (!SymbolEqualityComparer.Default.Equals(
                attr.AttributeClass, System_Reflection_AssemblyMetadataAttribute))
                return null;
            var args = attr.ConstructorArguments;
            if (args.Length == 2
                && args[0].Value is string key
                && args[1].Value is string val)
                return new(key, val);
            return null;
        }
    }
}
