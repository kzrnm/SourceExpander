using System.Collections.Immutable;
using System.Runtime.Serialization;

namespace SourceExpander
{
    internal partial class EmbedderConfig
    {
        public EmbedderConfig(
            bool enabled = true,
            EmbeddingType embeddingType = EmbeddingType.GZipBase32768,
            string[]? excludeAttributes = null,
            MinifyLevel minifyLevel = MinifyLevel.Default,
            string[]? removeConditional = null,
            EmbeddingSourceClass? embeddingSourceClass = null,
            ImmutableArray<ObsoleteConfigProperty> obsoleteConfigProperties = default)
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
            ObsoleteConfigProperties = obsoleteConfigProperties.IsDefault ? ImmutableArray<ObsoleteConfigProperty>.Empty : obsoleteConfigProperties;
        }

        public bool Enabled { get; }
        public EmbeddingType EmbeddingType { get; }
        public ImmutableHashSet<string> ExcludeAttributes { get; }
        public MinifyLevel MinifyLevel { get; }
        public ImmutableHashSet<string> RemoveConditional { get; }
        public EmbeddingSourceClass EmbeddingSourceClass { get; }
        public ImmutableArray<ObsoleteConfigProperty> ObsoleteConfigProperties { get; }

        public static EmbedderConfig Parse(string sourceText)
        {
            if (JsonUtil.ParseJson<EmbedderConfig>(sourceText) is { } config)
                return config;
            return new EmbedderConfig();
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
    public class ObsoleteConfigProperty
    {
        public static ObsoleteConfigProperty EnableMinify { get; }
            = new("enable-minify", "minify-level");

        public string Name { get; }
        public string Instead { get; }
        private ObsoleteConfigProperty(string name, string instead)
        {
            Name = name;
            Instead = instead;
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
}
