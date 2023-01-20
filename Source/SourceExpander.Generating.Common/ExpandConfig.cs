using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;

namespace SourceExpander
{
    /// <summary>
    /// Config of expanding
    /// </summary>
    /// <param name="Enabled">if true, Generator is enebled.</param>
    /// <param name="MatchFilePatterns">if a file path contains any item of<see cref="MatchFilePatterns"/>, Generator resolve the file's dependency.</param>
    /// <param name="IgnoreFilePatterns">if a file path matches any item of<see cref="IgnoreFilePatterns"/>, Generator doesn't resolve the file's dependency.</param>
    /// <param name="IgnoreAssemblies">if a name of assembly matches any item of <see cref="IgnoreAssemblies"/>, Generator doesn't expand sources of the assembly.</param>
    /// <param name="StaticEmbeddingText">static text that be added to source code.</param>
    /// <param name="MetadataExpandingFile"> file path whose source code is written to metadata</param>
    /// <param name="ExpandingByGroup">if true, generator write `#region &lt;AssemblyName&gt;`.</param>
    public partial record ExpandConfig(
         bool Enabled,
         ImmutableArray<string> MatchFilePatterns,
         ImmutableArray<Regex> IgnoreFilePatterns,
         ImmutableArray<string> IgnoreAssemblies,
         string? StaticEmbeddingText,
         string? MetadataExpandingFile,
         bool ExpandingByGroup)
    {
        /// <summary>
        /// constructor
        /// </summary>
        public ExpandConfig(
            bool enabled = true,
            string[]? matchFilePatterns = null,
            string[]? ignoreAssemblies = null,
            IEnumerable<Regex>? ignoreFilePatterns = null,
            string? staticEmbeddingText = null,
            string? metadataExpandingFile = null,
            bool? expandingByGroup = null) :
        this(
            Enabled: enabled,
            MatchFilePatterns: matchFilePatterns is null
                ? ImmutableArray<string>.Empty
                : ImmutableArray.Create(matchFilePatterns),
            IgnoreFilePatterns: ignoreFilePatterns is null
                ? ImmutableArray<Regex>.Empty
                : ImmutableArray.CreateRange(ignoreFilePatterns),
            IgnoreAssemblies: ignoreAssemblies is null
                ? ImmutableArray<string>.Empty
                : ImmutableArray.Create(ignoreAssemblies),
            StaticEmbeddingText: staticEmbeddingText,
            MetadataExpandingFile: metadataExpandingFile,
            ExpandingByGroup: expandingByGroup ?? false
        )
        { }

        /// <summary>
        /// whether Generator resolve source code of <paramref name="filePath"/>.
        /// </summary>
        public bool IsMatch(string filePath)
            => (MatchFilePatterns.Length == 0
                || MatchFilePatterns.Any(p => filePath.IndexOf(p, StringComparison.OrdinalIgnoreCase) >= 0))
                && IgnoreFilePatterns.All(regex => !regex.IsMatch(filePath));
    }
}
