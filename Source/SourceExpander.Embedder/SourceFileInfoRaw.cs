using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace SourceExpander
{
    internal class SourceFileInfoRaw
    {
        public SyntaxTree SyntaxTree { get; }
        public string FileName { get; }
        public ImmutableHashSet<string> DefinedTypeNames { get; }
        public ImmutableHashSet<string> UsedTypeNames { get; }
        public ImmutableHashSet<string> Usings { get; }
        public string CodeBody { get; }
        public SourceFileInfoRaw WithFileName(string newName)
            => new(
                SyntaxTree,
                newName,
                DefinedTypeNames,
                UsedTypeNames,
                Usings,
                CodeBody);

        public SourceFileInfo Resolve(Dictionary<string, HashSet<string>> dependencyInfo)
        {
            var deps = new HashSet<string>();
            foreach (var type in this.UsedTypeNames)
                if (dependencyInfo.TryGetValue(type, out var defined))
                    deps.UnionWith(defined);
            deps.Remove(this.FileName);

            return new SourceFileInfo
                (
                    FileName,
                    DefinedTypeNames,
                    Usings,
                    deps,
                    CodeBody
                );
        }

        public SourceFileInfoRaw(
            SyntaxTree syntaxTree,
            string fileName,
            ImmutableHashSet<string> definedTypeNames,
            ImmutableHashSet<string> usedTypeNames,
            ImmutableHashSet<string> usings,
            string codeBody)
        {
            SyntaxTree = syntaxTree;
            FileName = fileName;
            DefinedTypeNames = definedTypeNames;
            UsedTypeNames = usedTypeNames;
            Usings = usings;
            CodeBody = codeBody;
        }
    }
}
