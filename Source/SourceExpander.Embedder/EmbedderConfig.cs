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
            : this(
                  EmbeddingType.GZipBase32768,
                  Array.Empty<string>())
        { }
        public EmbedderConfig(
            EmbeddingType embeddingType,
            string[] excludeAttributes)
        {
            EmbeddingType = embeddingType;
            ExcludeAttributes = ImmutableHashSet.Create(excludeAttributes);
        }

        public EmbeddingType EmbeddingType { get; }
        public ImmutableHashSet<string> ExcludeAttributes { get; }

        public static EmbedderConfig Parse(SourceText? sourceText, CancellationToken cancellationToken)
        {
            static EmbeddingType ParseEmbeddingType(string? str)
                => str?.ToLowerInvariant() switch
                {
                    "raw" => EmbeddingType.Raw,
                    _ => EmbeddingType.GZipBase32768,
                };

            try
            {
                if (sourceText is not null && JsonUtil.ParseJson<EmbedderConfigData>(sourceText, cancellationToken) is { } data)
                    return new EmbedderConfig(
                        embeddingType: ParseEmbeddingType(data.EmbeddingType),
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
            [DataMember(Name = "embedding-type")]
            public string? EmbeddingType { set; get; }
            [DataMember(Name = "exclude-attributes")]
            public string[]? ExcludeAttributes { set; get; }
        }
    }

    public enum EmbeddingType
    {
        GZipBase32768,
        Raw,
    }

#pragma warning disable CA1032
    internal sealed class ParseConfigException : Exception
    {
        public ParseConfigException() { }
        public ParseConfigException(Exception inner) : base(inner.Message, inner) { }
    }
}
