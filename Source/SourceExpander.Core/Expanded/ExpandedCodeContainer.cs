using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;

namespace SourceExpander.Expanded
{
    public class ExpandedCodeContainer : IReadOnlyDictionary<string, SourceCode>
    {
        private ExpandedCodeContainer() { }
        public static ExpandedCodeContainer Files = new ExpandedCodeContainer();

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Add(string path, SourceCode code) => dic.Add(Path.GetFullPath(path), code);


        private readonly Dictionary<string, SourceCode> dic = new Dictionary<string, SourceCode>();
        /// <summary>
        /// get expanded source code.
        /// </summary>
        /// <param name="filePath">absolute path of original source code</param>
        public SourceCode this[string filePath] => dic[filePath];

        public IEnumerable<string> Keys => dic.Keys;
        public IEnumerable<SourceCode> Values => dic.Values;
        public int Count => dic.Count;
        public bool ContainsKey(string key) => dic.ContainsKey(key);
        public IEnumerator<KeyValuePair<string, SourceCode>> GetEnumerator() => dic.GetEnumerator();
        public bool TryGetValue(string key, out SourceCode value) => dic.TryGetValue(key, out value);
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)dic).GetEnumerator();
    }
}
