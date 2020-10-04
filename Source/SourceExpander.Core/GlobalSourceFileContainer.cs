using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace SourceExpander
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class GlobalSourceFileContainer : IEnumerable<SourceFileInfo>, IEnumerable, IReadOnlyCollection<SourceFileInfo>
    {
        private static GlobalSourceFileContainer? _instance;
        public static GlobalSourceFileContainer Instance => _instance ??= new GlobalSourceFileContainer();
        private GlobalSourceFileContainer() { }
        private readonly List<SourceFileInfo> _sourceFileInfos = new List<SourceFileInfo>();
        private readonly List<Func<SourceFileInfo>> _addFuncs = new List<Func<SourceFileInfo>>();
        private readonly List<Func<IEnumerable<SourceFileInfo>>> _addRangeFuncs = new List<Func<IEnumerable<SourceFileInfo>>>();
        public int Count
        {
            get
            {
                Update();
                return _sourceFileInfos.Count;
            }
        }

        public void AddLazy(Func<SourceFileInfo> addFunc) => _addFuncs.Add(addFunc);
        public void AddLazy(Func<IEnumerable<SourceFileInfo>> addRangeFunc) => _addRangeFuncs.Add(addRangeFunc);

        public List<SourceFileInfo>.Enumerator GetEnumerator()
        {
            Update();
            return _sourceFileInfos.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        IEnumerator<SourceFileInfo> IEnumerable<SourceFileInfo>.GetEnumerator() => GetEnumerator();
        private void Update()
        {
            foreach (var f in _addFuncs)
                _sourceFileInfos.Add(f());
            foreach (var f in _addRangeFuncs)
                _sourceFileInfos.AddRange(f());

            _addFuncs.Clear();
            _addRangeFuncs.Clear();
        }
    }
}
