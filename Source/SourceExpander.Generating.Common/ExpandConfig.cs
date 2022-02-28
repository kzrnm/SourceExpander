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
    public partial class ExpandConfig
    {
        /// <summary>
        /// constructor
        /// </summary>
        public ExpandConfig(
            bool enabled = true,
            IEnumerable<string>? matchFilePatterns = null,
            IEnumerable<Regex>? ignoreFilePatterns = null,
            string? staticEmbeddingText = null,
            string? metadataExpandingFile = null,
            IEnumerable<ReplacingConfig>? replacings = null)
        {
            Enabled = enabled;
            MatchFilePatterns = matchFilePatterns?.ToImmutableArray() ?? ImmutableArray<string>.Empty;
            IgnoreFilePatterns = ignoreFilePatterns?.ToImmutableArray() ?? ImmutableArray<Regex>.Empty;
            StaticEmbeddingText = staticEmbeddingText;
            MetadataExpandingFile = metadataExpandingFile;
            ReplacingConfigs = replacings?.ToImmutableArray() ?? ImmutableArray<ReplacingConfig>.Empty;
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
        /// replace string in source code
        /// </summary>
        public ImmutableArray<ReplacingConfig> ReplacingConfigs { get; }
        /// <summary>
        /// whether Generator resolve source code of <paramref name="filePath"/>.
        /// </summary>
        public bool IsMatch(string filePath)
            => (MatchFilePatterns.Length == 0
                || MatchFilePatterns.Any(p => filePath.IndexOf(p, StringComparison.OrdinalIgnoreCase) >= 0))
                && IgnoreFilePatterns.All(regex => !regex.IsMatch(filePath));
    }

    /// <summary>
    /// Use in <see cref="ReplacingConfig"/>
    /// </summary>
    public enum ReplacingType
    {
        /// <summary>
        /// Replace using string
        /// </summary>
        String,
        /// <summary>
        /// Replace using regular expression
        /// </summary>
        Regex,
    }
    /// <summary>
    /// Use in <see cref="ExpandConfig.ReplacingConfigs"/>
    /// </summary>
    public class ReplacingConfig
    {
        /// <summary>
        /// constructor
        /// </summary>
        public ReplacingConfig(
            string oldString,
            string replacement,
            ReplacingType replacingType
            )
        {
            OldString = oldString;
            Replacement = replacement;
            ReplacingType = replacingType;
        }
        /// <summary>
        /// Replace method type
        /// </summary>
        public ReplacingType ReplacingType { get; }
        /// <summary>
        /// target of replacing
        /// </summary>
        public string OldString { get; }
        /// <summary>
        /// replacement 
        /// </summary>
        public string Replacement { get; }
    }
}
