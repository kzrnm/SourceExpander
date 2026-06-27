using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace SourceExpander;

[GeneratorConfig]
public partial record ExpandConfig
{
    private class ExpandConfigData
    {
        [JsonProperty("enabled")]
        public bool? Enabled;
        [JsonProperty("match-file-pattern")]
        public string[]? MatchFilePattern;
        [JsonProperty("metadata-expanding-file")]
        public string? MetadataExpandingFile;
        [JsonProperty("ignore-file-pattern-regex")]
        public string[]? IgnoreFilePatternRegex;
        [JsonProperty("ignore-assemblies")]
        public string[]? IgnoreAssemblies;
        [JsonProperty("static-embedding-text")]
        public string? StaticEmbeddingText;
        [JsonProperty("expanding-all")]
        public bool? ExpandingAll;
        [JsonProperty("expanding-by-group")]
        public bool? ExpandingByGroup;
        [JsonProperty("expanding-position")]
        public string? ExpandingPosition;
        private ExpandingPosition ParsedExpandingPosition
            => Enum.TryParse(ExpandingPosition, true, out ExpandingPosition r) ? r : SourceExpander.ExpandingPosition.EndOfFile;

        public ExpandConfig ToImmutable() => new(
                enabled: this.Enabled ?? true,
                matchFilePatterns: this.MatchFilePattern ?? Array.Empty<string>(),
                ignoreAssemblies: this.IgnoreAssemblies ?? Array.Empty<string>(),
                ignoreFilePatterns: this.IgnoreFilePatternRegex?.Select(s => new Regex(s)) ?? Array.Empty<Regex>(),
                staticEmbeddingText: this.StaticEmbeddingText,
                metadataExpandingFile: MetadataExpandingFile,
                expandingAll: ExpandingAll,
                expandingPosition: ParsedExpandingPosition,
                expandingByGroup: ExpandingByGroup);
    }
}
