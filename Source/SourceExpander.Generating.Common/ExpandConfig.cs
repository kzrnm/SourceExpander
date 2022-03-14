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
    public partial class ExpandConfig : IEquatable<ExpandConfig?>
    {
        /// <summary>
        /// constructor
        /// </summary>
        public ExpandConfig(
            bool enabled = true,
            string[]? matchFilePatterns = null,
            IEnumerable<Regex>? ignoreFilePatterns = null,
            string? staticEmbeddingText = null,
            string? metadataExpandingFile = null)
        {
            Enabled = enabled;
            MatchFilePatterns = matchFilePatterns is null
                ? ImmutableArray<string>.Empty
                : ImmutableArray.Create(matchFilePatterns);
            IgnoreFilePatterns = ignoreFilePatterns is null
                ? ImmutableArray<Regex>.Empty
                : ImmutableArray.CreateRange(ignoreFilePatterns);
            StaticEmbeddingText = staticEmbeddingText;
            MetadataExpandingFile = metadataExpandingFile;
        }

        /// <summary>
        /// if true, Generator is enebled.
        /// </summary>
        public bool Enabled { get; }
        /// <summary>
        /// if a file path contains any item of <see cref="MatchFilePatterns"/>, Generator resolve the file's dependency.
        /// </summary>
        public ImmutableArray<string> MatchFilePatterns { get; }
        /// <summary>
        /// if a file path matches any item of <see cref="IgnoreFilePatterns"/>, Generator doesn't resolve the file's dependency.
        /// </summary>
        public ImmutableArray<Regex> IgnoreFilePatterns { get; }
        /// <summary>
        /// static text that be added to source code.
        /// </summary>
        public string? StaticEmbeddingText { get; }
        /// <summary>
        /// file path whose source code is written to metadata
        /// </summary>
        public string? MetadataExpandingFile { get; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override bool Equals(object? obj) => Equals(obj as ExpandConfig);
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool Equals(ExpandConfig? other) => other != null && Enabled == other.Enabled
            && MatchFilePatterns.Equals(other.MatchFilePatterns)
            && IgnoreFilePatterns.Equals(other.IgnoreFilePatterns)
            && StaticEmbeddingText == other.StaticEmbeddingText
            && MetadataExpandingFile == other.MetadataExpandingFile;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override int GetHashCode()
        {
            int hashCode = -262998452;
            hashCode = hashCode * -1521134295 + Enabled.GetHashCode();
            hashCode = hashCode * -1521134295 + MatchFilePatterns.GetHashCode();
            hashCode = hashCode * -1521134295 + IgnoreFilePatterns.GetHashCode();
            hashCode = hashCode * -1521134295 + (StaticEmbeddingText?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (MetadataExpandingFile?.GetHashCode() ?? 0);
            return hashCode;
        }

        /// <summary>
        /// whether Generator resolve source code of <paramref name="filePath"/>.
        /// </summary>
        public bool IsMatch(string filePath)
            => (MatchFilePatterns.Length == 0
                || MatchFilePatterns.Any(p => filePath.IndexOf(p, StringComparison.OrdinalIgnoreCase) >= 0))
                && IgnoreFilePatterns.All(regex => !regex.IsMatch(filePath));
    }
}
