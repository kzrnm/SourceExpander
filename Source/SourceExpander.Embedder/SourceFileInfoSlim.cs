using System.Collections.Generic;
using System.Collections.Immutable;

namespace SourceExpander
{
    internal class SourceFileInfoSlim
    {
        public string FileName { get; }
        public ImmutableHashSet<string> TypeNames { get; }

        public SourceFileInfoSlim(SourceFileInfo file) : this(file.FileName, file.TypeNames) { }
        public SourceFileInfoSlim(SourceFileInfoRaw file) : this(file.FileName, file.TypeNames) { }
        public SourceFileInfoSlim(string filename, IEnumerable<string> typeNames)
        {
            FileName = filename;
            TypeNames = ImmutableHashSet.CreateRange(typeNames);
        }
    }
}
