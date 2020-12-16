using System;
using System.Collections.Immutable;
using System.Runtime.Serialization;
using System.Threading;
using Microsoft.CodeAnalysis.Text;

namespace SourceExpander
{
    public class EmbedderConfig
    {
        public EmbedderConfig(
            bool enabled = true,
            EmbeddingType embeddingType = EmbeddingType.GZipBase32768,
            string[]? excludeAttributes = null,
            bool enableMinify = false,
            string[]? removeConditional = null)
        {
            Enabled = enabled;
            EmbeddingType = embeddingType;
            EnableMinify = enableMinify;
            ExcludeAttributes = excludeAttributes switch
            {
                null => ImmutableHashSet<string>.Empty,
                _ => ImmutableHashSet.Create(excludeAttributes),
            };
            RemoveConditional = removeConditional switch
            {
                null => ImmutableHashSet<string>.Empty,
                _ => ImmutableHashSet.Create(removeConditional),
            };
        }

        public bool Enabled { get; }
        public EmbeddingType EmbeddingType { get; }
        public ImmutableHashSet<string> ExcludeAttributes { get; }
        public bool EnableMinify { get; }
        public ImmutableHashSet<string> RemoveConditional { get; }

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
                        enabled: data.Enabled ?? true,
                        embeddingType: ParseEmbeddingType(data.EmbeddingType),
                        excludeAttributes: data.ExcludeAttributes,
                        removeConditional: data.RemoveConditional,
                        enableMinify: data.EnableMinify == true);
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
            [DataMember(Name = "enabled")]
            public bool? Enabled { set; get; }
            [DataMember(Name = "embedding-type")]
            public string? EmbeddingType { set; get; }
            [DataMember(Name = "exclude-attributes")]
            public string[]? ExcludeAttributes { set; get; }
            [DataMember(Name = "enable-minify")]
            public bool? EnableMinify { set; get; }
            [DataMember(Name = "remove-conditional")]
            public string[]? RemoveConditional { set; get; }
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
