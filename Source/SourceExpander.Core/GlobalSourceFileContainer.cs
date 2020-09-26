using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace SourceExpander
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class GlobalSourceFileContainer : IEnumerable<SourceFileInfo>, IEnumerable, IReadOnlyCollection<SourceFileInfo>
    {
        public static GlobalSourceFileContainer Instance { get; } = new GlobalSourceFileContainer();
        private GlobalSourceFileContainer() { }
        private readonly List<SourceFileInfo> _sourceFileInfos = new List<SourceFileInfo>();
        public int Count => _sourceFileInfos.Count;
        public void Add(SourceFileInfo sourceFileInfo) => _sourceFileInfos.Add(sourceFileInfo);
        public void AddRange(IEnumerable<SourceFileInfo> sourceFileInfos) => _sourceFileInfos.AddRange(sourceFileInfos); public void Clear() => _sourceFileInfos.Clear();
        public List<SourceFileInfo>.Enumerator Enumerator() => _sourceFileInfos.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _sourceFileInfos.GetEnumerator();
        IEnumerator<SourceFileInfo> IEnumerable<SourceFileInfo>.GetEnumerator() => _sourceFileInfos.GetEnumerator();
    }
}
