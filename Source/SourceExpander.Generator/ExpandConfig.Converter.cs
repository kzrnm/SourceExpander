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
        public bool? Enabled { set; get; }
        [JsonProperty("match-file-pattern")]
        public string[]? MatchFilePattern { set; get; }
        [JsonProperty("metadata-expanding-file")]
        public string? MetadataExpandingFile { set; get; }
        [JsonProperty("ignore-file-pattern-regex")]
        public string[]? IgnoreFilePatternRegex { set; get; }
        [JsonProperty("ignore-assemblies")]
        public string[]? IgnoreAssemblies { set; get; }
        [JsonProperty("static-embedding-text")]
        public string? StaticEmbeddingText { set; get; }
        [JsonProperty("expanding-all")]
        public bool? ExpandingAll { set; get; }
        [JsonProperty("expanding-by-group")]
        public bool? ExpandingByGroup { set; get; }
        [JsonProperty("expanding-position")]
        public string? ExpandingPosition { set; get; }
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
