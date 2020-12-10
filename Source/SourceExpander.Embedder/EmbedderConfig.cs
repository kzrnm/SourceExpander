using System;
using System.Collections.Immutable;
using System.Runtime.Serialization;
using System.Threading;
using Microsoft.CodeAnalysis.Text;

namespace SourceExpander
{
    public class EmbedderConfig
    {
        public EmbedderConfig()
            : this(Array.Empty<string>()) { }
        public EmbedderConfig(string[] excludeAttributes)
        {
            ExcludeAttributes = ImmutableHashSet.Create(excludeAttributes);
        }
        public ImmutableHashSet<string> ExcludeAttributes { get; }

        public static EmbedderConfig Parse(SourceText? sourceText, CancellationToken cancellationToken)
        {
            if (sourceText is not null && JsonUtil.ParseJson<EmbedderConfigData>(sourceText, cancellationToken) is { } data)
                return new EmbedderConfig(
                    excludeAttributes: data.ExcludeAttributes ?? Array.Empty<string>());
            return new EmbedderConfig();
        }

        [DataContract]
        private class EmbedderConfigData
        {
            public ExtensionDataObject? ExtensionData { get; set; }
            [DataMember(Name = "exclude-attributes")]
            public string[]? ExcludeAttributes { set; get; }
        }
    }
}
