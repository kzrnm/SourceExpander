using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace SourceExpander
{
    internal partial class EmbedderConfig : IEquatable<EmbedderConfig?>
    {
        public EmbedderConfig(
            bool enabled = true,
            EmbeddingType embeddingType = EmbeddingType.GZipBase32768,
            string[]? excludeAttributes = null,
            MinifyLevel minifyLevel = MinifyLevel.Default,
            string[]? removeConditional = null,
            EmbeddingSourceClass? embeddingSourceClass = null,
            EmbeddingFileNameType embeddingFileNameType = EmbeddingFileNameType.WithoutCommonPrefix,
            string? projectDir = null,
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
            EmbeddingFileNameType = embeddingFileNameType;
            ProjectDir = projectDir ?? string.Empty;
            ObsoleteConfigProperties = obsoleteConfigProperties.IsDefault ? ImmutableArray<ObsoleteConfigProperty>.Empty : obsoleteConfigProperties;
        }

        public bool Enabled { get; }
        /// <summary>
        /// GZipBase32768 or Raw
        /// </summary>
        public EmbeddingType EmbeddingType { get; }
        /// <summary>
        /// Attribute full name that remove on embedding.
        /// </summary>
        public ImmutableHashSet<string> ExcludeAttributes { get; }
        /// <summary>
        /// Minify level of source code.
        /// </summary>
        public MinifyLevel MinifyLevel { get; }
        /// <summary>
        /// Remove method with ConditionalAttribute whose argument is in <see cref="RemoveConditional"/>.
        /// </summary>
        public ImmutableHashSet<string> RemoveConditional { get; }
        /// <summary>
        /// For debug. embedding source class.
        /// </summary>
        public EmbeddingSourceClass EmbeddingSourceClass { get; }
        /// <summary>
        /// Embedded file name type.
        /// </summary>
        public EmbeddingFileNameType EmbeddingFileNameType { get; }
        public ImmutableArray<ObsoleteConfigProperty> ObsoleteConfigProperties { get; }

        public string ProjectDir { get; }

        public override bool Equals(object? obj) => Equals(obj as EmbedderConfig);
        public bool Equals(EmbedderConfig? other) => other != null
            && Enabled == other.Enabled
            && EmbeddingType == other.EmbeddingType
            && MinifyLevel == other.MinifyLevel
            && ExcludeAttributes.SetEquals(other.ExcludeAttributes)
            && RemoveConditional.SetEquals(other.RemoveConditional)
            && EmbeddingSourceClass.Equals(other.EmbeddingSourceClass)
            && EmbeddingFileNameType == other.EmbeddingFileNameType
            && ObsoleteConfigProperties.Equals(other.ObsoleteConfigProperties);

        public override int GetHashCode()
        {
            int hashCode = -608788792;
            hashCode = hashCode * -1521134295 + Enabled.GetHashCode();
            hashCode = hashCode * -1521134295 + EmbeddingType.GetHashCode();
            hashCode = hashCode * -1521134295 + MinifyLevel.GetHashCode();
            hashCode = hashCode * -1521134295 + ExcludeAttributes.FirstOrDefault()?.GetHashCode() ?? 0;
            hashCode = hashCode * -1521134295 + RemoveConditional.FirstOrDefault()?.GetHashCode() ?? 0;
            hashCode = hashCode * -1521134295 + EqualityComparer<EmbeddingSourceClass>.Default.GetHashCode(EmbeddingSourceClass);
            hashCode = hashCode * -1521134295 + EmbeddingFileNameType.GetHashCode();
            hashCode = hashCode * -1521134295 + ObsoleteConfigProperties.GetHashCode();
            return hashCode;
        }
    }
    public class ObsoleteConfigProperty : IEquatable<ObsoleteConfigProperty?>
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

        public override bool Equals(object? obj) => Equals(obj as ObsoleteConfigProperty);
        public bool Equals(ObsoleteConfigProperty? other) => other != null && Name == other.Name && Instead == other.Instead;
        public override int GetHashCode()
        {
            int hashCode = -1743611513;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Instead);
            return hashCode;
        }
    }

    public class EmbeddingSourceClass : IEquatable<EmbeddingSourceClass?>
    {
        public EmbeddingSourceClass(bool enabled = false, string? className = null)
        {
            Enabled = enabled;
            ClassName = string.IsNullOrWhiteSpace(className) ? "SourceFileInfoContainer" : className!;
        }
        public bool Enabled { set; get; }
        public string ClassName { set; get; }

        public override bool Equals(object? obj) => Equals(obj as EmbeddingSourceClass);
        public bool Equals(EmbeddingSourceClass? other) => other != null && Enabled == other.Enabled && ClassName == other.ClassName;
        public override int GetHashCode()
        {
            int hashCode = -926497622;
            hashCode = hashCode * -1521134295 + Enabled.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ClassName);
            return hashCode;
        }

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

    public enum EmbeddingFileNameType
    {
        WithoutCommonPrefix,
        FullPath,
    }
}
