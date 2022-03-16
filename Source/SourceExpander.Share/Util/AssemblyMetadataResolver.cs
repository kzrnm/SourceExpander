using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;

namespace SourceExpander
{
    internal class AssemblyMetadataResolver
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

        public ImmutableDictionary<string, string> GetAssemblyMetadata(ISymbol symbol)
            => ImmutableDictionary.CreateRange(
                symbol.GetAttributes()
                .Select(GetAttributeSourceCode)
                .OfType<KeyValuePair<string, string>>());


        public (EmbeddedData Data, string? Name, ImmutableArray<(string Key, string ErrorMessage)> Errors)[] GetEmbeddedSourceFiles(bool includeSelf, CancellationToken cancellationToken)
        {
            var symbols = compilation.References
                .Select(r => (compilation.GetAssemblyOrModuleSymbol(r), r.Display));
            if (includeSelf)
            {
#if NETCOREAPP2_0_OR_GREATER
                symbols = symbols.Append((compilation.Assembly, compilation.AssemblyName));
#else
                symbols = symbols.Concat(new[] { ((ISymbol?)compilation.Assembly, compilation.AssemblyName) });
#endif
            }

            if (compilation.Options.ConcurrentBuild)
                return symbols.AsParallel(cancellationToken)
                    .Select(Load).ToArray();
            else
                return symbols.Do(_ => cancellationToken.ThrowIfCancellationRequested())
                    .Select(Load).ToArray();

            (EmbeddedData Data, string? Name, ImmutableArray<(string Key, string ErrorMessage)>)
                Load((ISymbol? symbol, string? name) tuple)
            {
                (ISymbol? symbol, string? name) = tuple;
                if (symbol is null)
                    return (EmbeddedData.Empty, name, ImmutableArray<(string, string)>.Empty);

                var (embedded, errors) = EmbeddedData.Create(
                    symbol.Name,
                    symbol.GetAttributes()
                    .Select(GetAttributeSourceCode)
                    .OfType<KeyValuePair<string, string>>());
                return (embedded, name, errors);
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
