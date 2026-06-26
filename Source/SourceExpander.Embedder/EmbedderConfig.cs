using System.Collections.Immutable;
using System.Linq;
using DotNet.Globbing;
using Microsoft.CodeAnalysis.CSharp;

namespace SourceExpander;

/// <summary>
/// embedding config
/// </summary>
/// <param name="Enabled">if true, embedder is enabled.</param>
/// <param name="Include">Glob pattern of include files.</param>
/// <param name="Exclude">Glob pattern of exclude files.</param>
/// <param name="EmbeddingType">GZipBase32768 or Raw</param>
/// <param name="ExcludeAttributes">Fully qualified name of the attribute used to exclude items from embedding.</param>
/// <param name="MinifyLevel">Minify level of source code.</param>
/// <param name="RemoveConditional">Remove method calls annotated with ConditionalAttribute whose argument is in <see cref="RemoveConditional"/>.</param>
/// <param name="EmbeddingSourceClassName">For debug. If not null, the generator embed source class with the class name.</param>
/// <param name="EmbeddingFileNameType">Embedded file name type.</param>
/// <param name="ObsoleteConfigProperties">Obsolete config property in json.</param>
/// <param name="LanguageVersion">C# version of embedded sources.</param>
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
     LanguageVersion? LanguageVersion,
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
        LanguageVersion? languageVersion = null,
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
            LanguageVersion: languageVersion,
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

/// <summary>
/// Obsolete property. It is retained only to produce warnings and should not be used by users.
/// </summary>
/// <param name="Name">Property name</param>
/// <param name="Instead">Replacement property</param>
public record ObsoleteConfigProperty(string Name, string Instead)
{
    /// <summary>
    /// Whether to enable minification.
    /// </summary>
    public static ObsoleteConfigProperty EnableMinify { get; } = new("enable-minify", "minify-level");
    /// <summary>
    /// Name of the embedded source class.
    /// </summary>
    public static ObsoleteConfigProperty EmbeddingSourceClass { get; } = new("embedding-source-class", "embedding-source-class-name");
    /// <summary>
    /// Expand in library when the symbol is defined.
    /// </summary>
    public static ObsoleteConfigProperty ExpandingSymbol { get; } = new("expanding-symbol", "expand-in-library");
}

/// <summary>
/// Minification level
/// </summary>
public enum MinifyLevel
{
    /// <summary>
    /// Collapse consecutive whitespace characters into a single space.
    /// </summary>
    Default,
    /// <summary>
    /// Do not minify.
    /// </summary>
    Off,
    /// <summary>
    /// Remove as much whitespace as possible.
    /// </summary>
    Full,
}

/// <summary>
/// Embedding format type
/// </summary>
public enum EmbeddingType
{
    /// <summary>
    /// Embed the GZip-compressed data as a Base32768-encoded string(default).
    /// </summary>
    GZipBase32768,
    /// <summary>
    /// Embed the JSON string as-is.
    /// </summary>
    Raw,
}

/// <summary>
/// Format of embedded file names.
/// </summary>
public enum EmbeddingFileNameType
{
    /// <summary>
    /// Embed the file path after removing the common path prefix(default).
    /// </summary>
    WithoutCommonPrefix,
    /// <summary>
    /// Embed the full file path.
    /// </summary>
    FullPath,
}
