#nullable disable
using System.Collections;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SourceExpander
{
    public class DummyAnalyzerConfigOptionsProvider2 : AnalyzerConfigOptionsProvider, IEnumerable<KeyValuePair<string, string>>
    {
        public void Add(string key, string value) => impl.dict.Add(key, value);
        private readonly DummyAnalyzerConfigOptions impl = new();
        public override AnalyzerConfigOptions GlobalOptions => impl;
        public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => impl;
        public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) => impl;
        IEnumerator<KeyValuePair<string, string>> IEnumerable<KeyValuePair<string, string>>.GetEnumerator() => impl.dict.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => impl.dict.GetEnumerator();

        private class DummyAnalyzerConfigOptions : AnalyzerConfigOptions
        {
            public readonly Dictionary<string, string> dict = new(KeyComparer);
            public override bool TryGetValue(string key, out string value) => dict.TryGetValue(key, out value);
        }
    }
}
