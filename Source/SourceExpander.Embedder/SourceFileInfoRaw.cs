using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace SourceExpander
{
    internal class SourceFileInfoRaw(
        SyntaxTree syntaxTree,
        string fileName,
        ImmutableHashSet<string> definedTypeNames,
        ImmutableHashSet<INamedTypeSymbol> usedTypes,
        ImmutableHashSet<string> usings,
        string codeBody,
        bool @unsafe = false)
    {
        public SyntaxTree SyntaxTree { get; } = syntaxTree;
        public string FileName { get; } = fileName;
        public ImmutableHashSet<string> DefinedTypeNames { get; } = definedTypeNames;
        public ImmutableHashSet<INamedTypeSymbol> UsedTypes { get; } = usedTypes;
        public ImmutableHashSet<string> Usings { get; } = usings;
        public string CodeBody { get; } = codeBody;
        public bool Unsafe { get; } = @unsafe;
        public SourceFileInfoRaw WithFileName(string newName)
            => new(
                SyntaxTree,
                newName,
                DefinedTypeNames,
                UsedTypes,
                Usings,
                CodeBody,
                Unsafe);

        public SourceFileInfo Resolve(Dictionary<string, HashSet<string>> dependencyInfo)
        {
            var deps = new HashSet<string>();
            foreach (var type in this.UsedTypes)
            {
                var typeName = type.ToDisplayString();
                if (dependencyInfo.TryGetValue(typeName, out var defined))
                    deps.UnionWith(defined);
            }
            deps.Remove(this.FileName);

            return new SourceFileInfo
                (
                    FileName,
                    DefinedTypeNames,
                    Usings,
                    deps,
                    CodeBody,
                    Unsafe
                );
        }
    }
}
