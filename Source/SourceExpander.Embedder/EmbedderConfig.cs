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
            try
            {
                if (sourceText is not null && JsonUtil.ParseJson<EmbedderConfigData>(sourceText, cancellationToken) is { } data)
                    return new EmbedderConfig(
                        excludeAttributes: data.ExcludeAttributes ?? Array.Empty<string>());
                return new EmbedderConfig();
            }
            catch (Exception e)
            {
                throw new ParseConfigException(e);
            }
        }

        [DataContract]
        private class EmbedderConfigData
        {
            public ExtensionDataObject? ExtensionData { get; set; }
            [DataMember(Name = "exclude-attributes")]
            public string[]? ExcludeAttributes { set; get; }
        }
    }

#pragma warning disable CA1032
    internal sealed class ParseConfigException : Exception
    {
        public ParseConfigException() { }
        public ParseConfigException(Exception inner) : base(inner.Message, inner) { }
    }
}
