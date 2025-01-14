﻿using System.Collections.Immutable;
using System.Linq;
using DotNet.Globbing;

namespace SourceExpander
{
    /// <summary>
    /// embedding config
    /// </summary>
    /// <param name="Enabled">if true, embedder is enabled.</param>
    /// <param name="Include">Glob pattern of include files.</param>
    /// <param name="Exclude">Glob pattern of exclude files.</param>
    /// <param name="EmbeddingType">GZipBase32768 or Raw</param>
    /// <param name="ExcludeAttributes">Attribute full name that remove on embedding.</param>
    /// <param name="MinifyLevel">Minify level of source code.</param>
    /// <param name="RemoveConditional">Remove method with ConditionalAttribute whose argument is in <see cref="RemoveConditional"/>.</param>
    /// <param name="EmbeddingSourceClass">For debug. embedding source class.</param>
    /// <param name="EmbeddingFileNameType">Embedded file name type.</param>
    /// <param name="ObsoleteConfigProperties">Obsolete config property in json.</param>
    /// <param name="ExpandingSymbol">if <paramref name="ExpandingSymbol"/> is in preprocessor symbols, source codes will be expanded in the library.</param>
    internal partial record EmbedderConfig(
         bool Enabled,
         ImmutableArray<string> Include,
         ImmutableArray<string> Exclude,
         ImmutableArray<ObsoleteConfigProperty> ObsoleteConfigProperties,
         EmbeddingType EmbeddingType,
         ImmutableHashSet<string> ExcludeAttributes,
         MinifyLevel MinifyLevel,
         ImmutableHashSet<string> RemoveConditional,
         EmbeddingSourceClass EmbeddingSourceClass,
         EmbeddingFileNameType EmbeddingFileNameType,
         string? ExpandingSymbol
    )
    {
        public EmbedderConfig(
            bool enabled = true,
            string[]? include = null,
            string[]? exclude = null,
            EmbeddingType embeddingType = EmbeddingType.GZipBase32768,
            string[]? excludeAttributes = null,
            MinifyLevel minifyLevel = MinifyLevel.Default,
            string[]? removeConditional = null,
            EmbeddingSourceClass? embeddingSourceClass = null,
            EmbeddingFileNameType embeddingFileNameType = EmbeddingFileNameType.WithoutCommonPrefix,
            string? expandingSymbol = null,
            ImmutableArray<ObsoleteConfigProperty> obsoleteConfigProperties = default)
            : this(
                Enabled: enabled,
                Include: include switch { null => ImmutableArray<string>.Empty, _ => ImmutableArray.CreateRange(include) },
                Exclude: exclude switch { null => ImmutableArray<string>.Empty, _ => ImmutableArray.CreateRange(exclude) },
                ObsoleteConfigProperties: obsoleteConfigProperties.IsDefault ? ImmutableArray<ObsoleteConfigProperty>.Empty : obsoleteConfigProperties,
                EmbeddingType: embeddingType,
                MinifyLevel: minifyLevel,
                ExcludeAttributes: CreateImmutableHashSet(excludeAttributes),
                RemoveConditional: CreateImmutableHashSet(removeConditional),
                EmbeddingSourceClass: embeddingSourceClass ?? new EmbeddingSourceClass(false),
                EmbeddingFileNameType: embeddingFileNameType,
                ExpandingSymbol: expandingSymbol
            )
        {
        }

        private readonly ImmutableArray<Glob> IncludeGlobs = Include.Select(Glob.Parse).ToImmutableArray();
        private readonly ImmutableArray<Glob> ExcludeGlobs = Exclude.Select(Glob.Parse).ToImmutableArray();

        static ImmutableHashSet<string> CreateImmutableHashSet(string[]? a) => a switch
        {
            null => ImmutableHashSet<string>.Empty,
            _ => ImmutableHashSet.Create(a),
        };

        public bool IsMatch(string filePath)
        {
            if (IncludeGlobs.Length == 0 && ExcludeGlobs.Length == 0) return true;
            if (IncludeGlobs.Length > 0)
            {
                foreach (var g in IncludeGlobs)
                {
                    if (g.IsMatch(filePath))
                        goto INCLUDED;
                }
                return false;
            }
        INCLUDED:
            foreach (var g in ExcludeGlobs)
            {
                if (g.IsMatch(filePath))
                    return false;
            }
            return true;
        }
    }
    public record ObsoleteConfigProperty(string Name, string Instead)
    {
        public static ObsoleteConfigProperty EnableMinify { get; } = new("enable-minify", "minify-level");
    }

    public record EmbeddingSourceClass(bool Enabled = false, string? ClassName = null)
    {
        public bool Enabled { set; get; } = Enabled;
        public string ClassName { set; get; } = string.IsNullOrWhiteSpace(ClassName) ? "SourceFileInfoContainer" : ClassName!;

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
