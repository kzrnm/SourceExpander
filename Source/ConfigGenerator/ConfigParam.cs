using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SourceExpander;

internal record ConfigParams(ITypeSymbol Type, ImmutableArray<ConfigParam> Params) : IEnumerable<ConfigParam>
{
    public ImmutableArray<ConfigParam>.Enumerator GetEnumerator() => Params.GetEnumerator();
    IEnumerator<ConfigParam> IEnumerable<ConfigParam>.GetEnumerator() => ((IEnumerable<ConfigParam>)Params).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<ConfigParam>)Params).GetEnumerator();
}

internal record ConfigParam(string Name, string JsonName, ITypeSymbol Type, PropertyDeclarationSyntax Syntax, bool IsObsolete);
