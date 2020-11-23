using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
#nullable enable
namespace SourceExpander
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class SourceFileContainer : IEnumerable<SourceFileInfo>, IEnumerable, IReadOnlyCollection<SourceFileInfo>
    {
        private readonly Dictionary<string, SourceFileInfo> _sourceFiles;
        // TODO: DELETE
        public SourceFileContainer(IEnumerable<SourceFileInfo> origs)
        {
            _sourceFiles = new Dictionary<string, SourceFileInfo>();
            foreach (var sf in origs)
            {
                if (sf.FileName == null) throw new ArgumentException($"({nameof(sf.FileName)} is null");
                if (_sourceFiles.ContainsKey(sf.FileName))
                    throw new ArgumentException($"duplicate {nameof(sf.FileName)}: {sf.FileName}");
                _sourceFiles.Add(sf.FileName, sf);
            }
        }
        public SourceFileContainer(IEnumerable<EmbeddedData> embeddedDatas)
        {
            _sourceFiles = new Dictionary<string, SourceFileInfo>();
            foreach (var embedded in embeddedDatas)
                foreach (var sf in embedded.Sources)
                {
                    if (sf.FileName == null) throw new ArgumentException($"({nameof(sf.FileName)} is null");
                    if (_sourceFiles.ContainsKey(sf.FileName))
                        throw new ArgumentException($"duplicate {nameof(sf.FileName)}: {sf.FileName}");
                    _sourceFiles.Add(sf.FileName, sf);
                }
        }

        public int Count => _sourceFiles.Count;
        public SourceFileInfo this[string key]
        {
            set
            {
                if (_sourceFiles.ContainsKey(key))
                    throw new ArgumentException($"duplicate {nameof(key)}: {key}");
                _sourceFiles.Add(key, value);
            }
            get => _sourceFiles[key];
        }
        public IEnumerable<string> Keys => _sourceFiles.Keys;
        public IEnumerable<SourceFileInfo> Values => _sourceFiles.Values;
        public bool ContainsKey(string fileName) => _sourceFiles.ContainsKey(fileName);
        public bool TryGetValue(string fileName, out SourceFileInfo? value) => _sourceFiles.TryGetValue(fileName, out value);
        public Dictionary<string, SourceFileInfo>.ValueCollection.Enumerator GetEnumerator() => _sourceFiles.Values.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _sourceFiles.Values.GetEnumerator();
        IEnumerator<SourceFileInfo> IEnumerable<SourceFileInfo>.GetEnumerator() => _sourceFiles.Values.GetEnumerator();

        /// <summary>
        /// <para>return <see cref="SourceFileInfo"/> that has TypeNames that overlaps <paramref name="typeNames"/>.</para>
        /// <para>if <paramref name="typeNameMatch"/> is true, <paramref name="typeNames"/> and <see cref="SourceFileInfo"/> is compared without namespace and type arguments.</para>
        /// <para>ex. AtCoder.INumOperator&lt;T&gt; → INumOperator</para>
        /// </summary>
        /// <param name="typeNames"></param>
        /// <param name="typeNameMatch"></param>
        /// <returns></returns>
        public IEnumerable<SourceFileInfo> ResolveDependency(IEnumerable<string> typeNames, bool typeNameMatch)
        {
            static string ToSimpleClassName(string typeName)
            {
                int l, r;
                // AtCoder.INumOperator<T> → INumOperator<T>
                for (l = typeName.Length - 1; l >= 0; l--)
                    if (typeName[l] == '.')
                        break;
                ++l;


                // INumOperator<T> → INumOperator
                for (r = l; r < typeName.Length; r++)
                    if (typeName[r] == '<')
                        break;

                return typeName.Substring(l, r - l);
            }
            IEnumerable<SourceFileInfo> ResolveSimpleName(IEnumerable<string> typeNames)
            {
                typeNames = typeNames.Select(ToSimpleClassName);
                var hs = new HashSet<string>(typeNames);
                foreach (var s in _sourceFiles.Values)
                    if (hs.Overlaps(s.TypeNames.Select(ToSimpleClassName)))
                        yield return s;
            }
            IEnumerable<SourceFileInfo> ResolveFullName(IEnumerable<string> typeNames)
            {
                var hs = new HashSet<string>(typeNames);
                foreach (var s in _sourceFiles.Values)
                    if (hs.Overlaps(s.TypeNames))
                        yield return s;
            }

            if (typeNameMatch)
                return ResolveDependency(ResolveSimpleName(typeNames));
            else
                return ResolveDependency(ResolveFullName(typeNames));
        }
        private IEnumerable<SourceFileInfo> ResolveDependency(IEnumerable<SourceFileInfo> origs)
        {
            var result = new List<SourceFileInfo>();
            var fileNameQueue = new Queue<string>();
            var usedFileName = new HashSet<string>();

            foreach (var s in origs)
            {
                if (s.FileName == null) throw new ArgumentException($"({nameof(s.FileName)} is null");
                usedFileName.Add(s.FileName);
                result.Add(s);
            }
            foreach (var d in origs.SelectMany(s => s.Dependencies))
                if (usedFileName.Add(d))
                    fileNameQueue.Enqueue(d);

            while (fileNameQueue.Count > 0)
            {
                var dep = fileNameQueue.Dequeue();
                if (_sourceFiles.TryGetValue(dep, out var s))
                {
                    result.Add(s);
                    if (s.Dependencies == null) throw new ArgumentException($"({nameof(s.Dependencies)} is null");
                    foreach (var d in s.Dependencies)
                        if (usedFileName.Add(d))
                            fileNameQueue.Enqueue(d);
                }
            }

            return result;
        }
    }
}
