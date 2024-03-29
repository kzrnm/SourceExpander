﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;

namespace SourceExpander
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class SourceFileContainer : IEnumerable<SourceFileInfo>, IEnumerable, IReadOnlyCollection<SourceFileInfo>
    {
        private readonly Dictionary<string, SourceFileInfo> _sourceFiles;
        private readonly Dictionary<string, List<SourceFileInfo>> _sourceFilesByTypeName;
        public SourceFileContainer(IEnumerable<EmbeddedData> embeddedDatas)
        {
            _sourceFiles = new();
            _sourceFilesByTypeName = new();
            var definedNamespacesBuilder = ImmutableHashSet.CreateBuilder<string>();
            foreach (var embedded in embeddedDatas)
            {
                foreach (var sf in embedded.Sources)
                {
                    if (sf.FileName == null) throw new ArgumentException($"({nameof(sf.FileName)} is null");
                    if (_sourceFiles.ContainsKey(sf.FileName))
                        throw new ArgumentException($"duplicate {nameof(sf.FileName)}: {sf.FileName}");
                    _sourceFiles.Add(sf.FileName, sf);

                    foreach (var type in sf.TypeNames)
                    {
                        if (!_sourceFilesByTypeName.TryGetValue(type, out var list))
                            _sourceFilesByTypeName[type] = list = new();
                        list.Add(sf);
                    }
                }
                definedNamespacesBuilder.UnionWith(embedded.EmbeddedNamespaces);
            }

            DefinedNamespaces = definedNamespacesBuilder.ToImmutable();
        }

        public ImmutableHashSet<string> DefinedNamespaces { get; }

        public int Count => _sourceFiles.Count;
        public SourceFileInfo this[string filename]
        {
            set
            {
                if (_sourceFiles.ContainsKey(filename))
                    throw new ArgumentException($"duplicate {nameof(filename)}: {filename}");
                _sourceFiles.Add(filename, value);
            }
            get => _sourceFiles[filename];
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
        /// <para>ex. AtCoder.INumOperator&lt;T&gt; → INumOperator</para>
        /// </summary>
        /// <returns></returns>
        public IEnumerable<SourceFileInfo> ResolveDependency(IEnumerable<INamedTypeSymbol> typeNames, CancellationToken cancellationToken = default)
            => ResolveDependency(
                typeNames.SelectMany(type => _sourceFilesByTypeName.TryGetValue(type.ToDisplayString(), out var list) ? list : Enumerable.Empty<SourceFileInfo>()),
                cancellationToken);
        private IEnumerable<SourceFileInfo> ResolveDependency(IEnumerable<SourceFileInfo> origs, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = new Dictionary<string, SourceFileInfo>();
            var fileNameQueue = new Queue<string>();

            cancellationToken.ThrowIfCancellationRequested();
            foreach (var s in origs)
            {
                if (s.FileName == null) throw new ArgumentException($"({nameof(s.FileName)} is null");
                result[s.FileName] = s;
            }
            foreach (var d in result.Values.SelectMany(s => s.Dependencies))
                if (!result.ContainsKey(d))
                    fileNameQueue.Enqueue(d);

            while (fileNameQueue.Count > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var dep = fileNameQueue.Dequeue();
                if (_sourceFiles.TryGetValue(dep, out var s))
                {
                    result[s.FileName] = s;
                    if (s.Dependencies == null) throw new ArgumentException($"({nameof(s.Dependencies)} is null");
                    foreach (var d in s.Dependencies)
                        if (!result.ContainsKey(d))
                            fileNameQueue.Enqueue(d);
                }
            }

            return result.Values;
        }
    }
}
