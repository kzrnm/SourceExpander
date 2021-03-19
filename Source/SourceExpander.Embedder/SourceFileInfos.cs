using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace SourceExpander
{

    internal interface ISourceFileInfoSlim
    {
        string FileName { get; }
        ImmutableHashSet<string> TypeNames { get; }
        ImmutableHashSet<string> UsedTypeNames { get; }
    }
    internal class SourceFileInfoRaw : ISourceFileInfoSlim
    {
        public SyntaxTree SyntaxTree { get; }
        public string FileName { get; }
        public ImmutableHashSet<string> TypeNames { get; }
        public ImmutableHashSet<string> UsedTypeNames { get; }
        public ImmutableHashSet<string> Usings { get; }
        public string CodeBody { get; }
        public SourceFileInfoRaw WithFileName(string newName)
            => new(
                SyntaxTree,
                newName,
                TypeNames,
                UsedTypeNames,
                Usings,
                CodeBody);

        public SourceFileInfoRaw(
            SyntaxTree syntaxTree,
            string fileName,
            ImmutableHashSet<string> typeNames,
            ImmutableHashSet<string> usedTypeNames,
            ImmutableHashSet<string> usings,
            string codeBody)
        {
            SyntaxTree = syntaxTree;
            FileName = fileName;
            TypeNames = typeNames;
            UsedTypeNames = usedTypeNames;
            Usings = usings;
            CodeBody = codeBody;
        }
    }
    internal class SourceFileInfoSlim : ISourceFileInfoSlim
    {
        public string FileName { get; }
        public ImmutableHashSet<string> TypeNames { get; }
        public ImmutableHashSet<string> UsedTypeNames => ImmutableHashSet<string>.Empty;

        public SourceFileInfoSlim(SourceFileInfo file) : this(file.FileName, file.TypeNames) { }
        public SourceFileInfoSlim(string filename, IEnumerable<string> typeNames)
        {
            FileName = filename;
            TypeNames = ImmutableHashSet.CreateRange(typeNames);
        }
    }
}

