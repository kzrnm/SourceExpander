using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace SourceExpander.Core
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class SourceFileInfo
    {
        public string FileName { get; }
        public ReadOnlyCollection<string> TypeNames { get; }
        public ReadOnlyCollection<string> Usings { get; }
        public ReadOnlyCollection<string> Dependencies { get; }
        public string CodeBody { get; }
        public SourceFileInfo(string fileName, IList<string> typeNames, IList<string> usings, IList<string> dependencies, string code)
        {
            FileName = fileName;
            TypeNames = new ReadOnlyCollection<string>(typeNames);
            Usings = new ReadOnlyCollection<string>(usings);
            Dependencies = new ReadOnlyCollection<string>(dependencies);
            CodeBody = code;
        }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class SourceFileContainer
    {
        private static readonly List<SourceFileInfo> _sourceFileInfos = new List<SourceFileInfo>();
        public static ReadOnlyCollection<SourceFileInfo> FileInfos { get; } = new ReadOnlyCollection<SourceFileInfo>(_sourceFileInfos);
        public static void Add(SourceFileInfo sourceFileInfo) => _sourceFileInfos.Add(sourceFileInfo);
        public static void AddRange(IEnumerable<SourceFileInfo> sourceFileInfos) => _sourceFileInfos.AddRange(sourceFileInfos);
    }
}
