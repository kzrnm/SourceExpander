using System.Collections.Immutable;
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
    /// <param name="EmbeddingSourceClassName">For debug. If not null, the generator embed source class with the class name.</param>
    /// <param name="EmbeddingFileNameType">Embedded file name type.</param>
    /// <param name="ObsoleteConfigProperties">Obsolete config property in json.</param>
    /// <param name="ExpandInLibrary">if true, source codes will be expanded in the library.</param>
    [GeneratorConfig]
    internal partial record EmbedderConfig(
         bool Enabled,
         ImmutableArray<string> Include,
         ImmutableArray<string> Exclude,
         ImmutableArray<ObsoleteConfigProperty> ObsoleteConfigProperties,
         EmbeddingType EmbeddingType,
         ImmutableHashSet<string> ExcludeAttributes,
         MinifyLevel MinifyLevel,
         ImmutableHashSet<string> RemoveConditional,
         string? EmbeddingSourceClassName,
         EmbeddingFileNameType EmbeddingFileNameType,
         bool ExpandInLibrary
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
            string? embeddingSourceClassName = null,
            EmbeddingFileNameType embeddingFileNameType = EmbeddingFileNameType.WithoutCommonPrefix,
            bool? expandInLibrary = null,
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
                EmbeddingSourceClassName: embeddingSourceClassName,
                EmbeddingFileNameType: embeddingFileNameType,
                ExpandInLibrary: expandInLibrary ?? false
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
        public static ObsoleteConfigProperty EmbeddingSourceClass { get; } = new("embedding-source-class", "embedding-source-class-name");
        public static ObsoleteConfigProperty ExpandingSymbol { get; } = new("expanding-symbol", "expand-in-library");
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
