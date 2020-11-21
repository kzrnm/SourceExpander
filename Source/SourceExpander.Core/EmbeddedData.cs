using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace SourceExpander
{
    public class EmbeddedData
    {
        public EmbeddedData(string assemblyName, Version embedderVersion, IReadOnlyList<SourceFileInfo> sources)
        {
            AssemblyName = assemblyName;
            EmbedderVersion = embedderVersion;
            Sources = sources;
        }
        public string AssemblyName { get; }
        public Version EmbedderVersion { get; }
        public IReadOnlyList<SourceFileInfo> Sources { get; }

        public bool IsEmpty => Sources.Count == 0;

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static EmbeddedData Create(string assemblyName, IEnumerable<KeyValuePair<string, string>> keyValuePairs)
        {
            Version version = new Version(1, 0, 0);
            var list = new List<SourceFileInfo>();
            foreach (var pair in keyValuePairs)
            {
                var attVer = SourceFileInfoUtil.GetAttributeEmbedderVersion(pair);
                if (attVer != null)
                {
                    version = attVer;
                }

                var attinfos = SourceFileInfoUtil.GetAttributeSourceFileInfos(pair);
                if (attinfos is null)
                    continue;

                list.AddRange(attinfos);
            }
            return new EmbeddedData(assemblyName, version, list.ToArray());
        }


    }
}
