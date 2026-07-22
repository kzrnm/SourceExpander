using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;

namespace SourceExpander;

internal record struct ParsedEmbeddedSource(EmbeddedData Data, string? Name, ImmutableArray<(string Key, string ErrorMessage)> Errors);

internal class AssemblyMetadataResolver(Compilation compilation)
{
    private INamedTypeSymbol System_Reflection_AssemblyMetadataAttribute
        => field ??= compilation.GetTypeByMetadataName("System.Reflection.AssemblyMetadataAttribute")
        ?? throw new Exception("System.Reflection.AssemblyMetadataAttribute is not found");

    public ImmutableDictionary<string, string> GetAssemblyMetadata(ISymbol symbol)
        => ImmutableDictionary.CreateRange(
            symbol.GetAttributes()
            .Select(GetAttributeSourceCode)
            .OfType<KeyValuePair<string, string>>());


    public ParsedEmbeddedSource[] GetEmbeddedSourceFiles(IEnumerable<EmbeddedData> preParsed, bool includeSelf, CancellationToken cancellationToken)
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

        return symbols
            .TryParallel(compilation.Options.ConcurrentBuild, cancellationToken)
            .Select(Load)
            .Concat(preParsed.Select(p => new ParsedEmbeddedSource(p, "<PreParsed>", ImmutableArray<(string, string)>.Empty)))
            .ToArray();

        ParsedEmbeddedSource Load((ISymbol? symbol, string? name) tuple)
        {
            (ISymbol? symbol, string? name) = tuple;
            if (symbol is null)
                return new(EmbeddedData.Empty, name, ImmutableArray<(string, string)>.Empty);

            var (embedded, errors) = EmbeddedData.LoadFromMetadata(
                symbol.Name,
                symbol.GetAttributes()
                .Select(GetAttributeSourceCode)
                .OfType<KeyValuePair<string, string>>());
            return new(embedded, name, errors);
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
