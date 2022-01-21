using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;

namespace SourceExpander
{
    internal partial class ExpandConfig
    {
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

        public bool Enabled { get; }
        public ImmutableArray<string> MatchFilePatterns { get; }
        public ImmutableArray<Regex> IgnoreFilePatterns { get; }
        public string? StaticEmbeddingText { get; }
        public string? MetadataExpandingFile { get; }
        public bool IsMatch(string filePath)
            => (MatchFilePatterns.Length == 0
                || MatchFilePatterns.Any(p => filePath.IndexOf(p, StringComparison.OrdinalIgnoreCase) >= 0))
                && IgnoreFilePatterns.All(regex => !regex.IsMatch(filePath));
    }
}
