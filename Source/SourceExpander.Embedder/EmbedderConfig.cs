using System;
using System.Collections.Immutable;
using System.Runtime.Serialization;
using Microsoft.CodeAnalysis.Text;

namespace SourceExpander
{
    public class EmbedderConfig
    {
        public EmbedderConfig(
            bool enabled = true,
            EmbeddingType embeddingType = EmbeddingType.GZipBase32768,
            string[]? excludeAttributes = null,
            MinifyLevel minifyLevel = MinifyLevel.Default,
            string[]? removeConditional = null,
            EmbeddingSourceClass? embeddingSourceClass = null)
        {
            Enabled = enabled;
            EmbeddingType = embeddingType;
            MinifyLevel = minifyLevel;
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
            EmbeddingSourceClass = embeddingSourceClass ?? new EmbeddingSourceClass(false);
        }

        public bool Enabled { get; }
        public EmbeddingType EmbeddingType { get; }
        public ImmutableHashSet<string> ExcludeAttributes { get; }
        public MinifyLevel MinifyLevel { get; }
        public ImmutableHashSet<string> RemoveConditional { get; }
        public EmbeddingSourceClass EmbeddingSourceClass { get; }
        public static EmbedderConfig Parse(SourceText? sourceText)
        {
            try
            {
                if (sourceText is not null
                    && JsonUtil.ParseJson<EmbedderConfigData>(sourceText) is { } data)
                    return data.ToImmutable();
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
            [DataMember(Name = "minify-level")]
            public string? MinifyLevel { set; get; }
            [DataMember(Name = "remove-conditional")]
            public string[]? RemoveConditional { set; get; }

            [DataMember(Name = "embedding-source-class")]
            public SourceClassData? EmbeddingSourceClass { set; get; }


            static EmbeddingType ParseEmbeddingType(string? str)
                => str?.ToLowerInvariant() switch
                {
                    "raw" => SourceExpander.EmbeddingType.Raw,
                    _ => SourceExpander.EmbeddingType.GZipBase32768,
                };

            static MinifyLevel ParseMinifyLevel(string? str)
                => str?.ToLowerInvariant() switch
                {
                    "full" => SourceExpander.MinifyLevel.Full,
                    "off" => SourceExpander.MinifyLevel.Off,
                    _ => SourceExpander.MinifyLevel.Default,
                };

            public EmbedderConfig ToImmutable() =>
                new(
                        Enabled ?? true,
                        ParseEmbeddingType(EmbeddingType),
                        ExcludeAttributes,
                        ParseMinifyLevel(MinifyLevel),
                        RemoveConditional,
                        EmbeddingSourceClass?.ToImmutable());
        }

        [DataContract]
        private class SourceClassData
        {
            [DataMember(Name = "enabled")]
            public bool? Enabled { set; get; }
            [DataMember(Name = "class-name")]
            public string? ClassName { set; get; }

            public EmbeddingSourceClass ToImmutable() => new(Enabled == true, ClassName);
        }
    }

    public class EmbeddingSourceClass
    {
        public EmbeddingSourceClass(bool enabled = false, string? className = null)
        {
            Enabled = enabled;
            ClassName = string.IsNullOrWhiteSpace(className) ? "SourceFileInfoContainer" : className!;
        }
        public bool Enabled { set; get; }
        public string ClassName { set; get; }

        public override string ToString() => Enabled ? $"class: {ClassName}" : "disable";
    }

    public enum MinifyLevel
    {
        Default,
        Off,
        Full,
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
