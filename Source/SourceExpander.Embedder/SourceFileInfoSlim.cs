using System.Collections.Generic;

namespace SourceExpander
{
    internal class SourceFileInfoSlim
    {
        public string FileName { get; }
        public HashSet<string> TypeNames { get; }

        public SourceFileInfoSlim(SourceFileInfo file) : this(file.FileName, file.TypeNames) { }
        public SourceFileInfoSlim(SourceFileInfoRaw file) : this(file.FileName, file.TypeNames) { }
        public SourceFileInfoSlim(string filename, IEnumerable<string> typeNames)
        {
            FileName = filename;
            TypeNames = new HashSet<string>(typeNames);
        }
    }
}
