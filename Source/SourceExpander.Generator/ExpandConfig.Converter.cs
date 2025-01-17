using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace SourceExpander
{
    public partial record ExpandConfig
    {
        [DataContract]
        private class ExpandConfigData
        {
            [DataMember(Name = "enabled")]
            public bool? Enabled { set; get; }
            [DataMember(Name = "match-file-pattern")]
            public string[]? MatchFilePattern { set; get; }
            [DataMember(Name = "metadata-expanding-file")]
            public string? MetadataExpandingFile { set; get; }
            [DataMember(Name = "ignore-file-pattern-regex")]
            public string[]? IgnoreFilePatternRegex { set; get; }
            [DataMember(Name = "ignore-assemblies")]
            public string[]? IgnoreAssemblies { set; get; }
            [DataMember(Name = "static-embedding-text")]
            public string? StaticEmbeddingText { set; get; }
            [DataMember(Name = "expanding-all")]
            public bool? ExpandingAll { set; get; }
            [DataMember(Name = "expanding-by-group")]
            public bool? ExpandingByGroup { set; get; }
            [DataMember(Name = "expanding-position")]
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
}
