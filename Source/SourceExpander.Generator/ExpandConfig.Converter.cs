using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace SourceExpander
{
    [JsonConverter(typeof(ExpandConfigConverter))]
    internal partial class ExpandConfig
    {
        public static ExpandConfig Parse(string sourceText)
        {
            if (JsonUtil.ParseJson<ExpandConfig>(sourceText) is { } config)
                return config;
            return new ExpandConfig();
        }

        private class ExpandConfigConverter : JsonConverter<ExpandConfig?>
        {
            public override bool CanWrite => false;
            public override ExpandConfig? ReadJson(JsonReader reader, Type objectType, ExpandConfig? existingValue, bool hasExistingValue, JsonSerializer serializer)
                => serializer.Deserialize<ExpandConfigData>(reader)?.ToImmutable();
            public override void WriteJson(JsonWriter writer, ExpandConfig? value, JsonSerializer serializer)
                => throw new NotImplementedException("CanWrite is always false");
        }

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
            [DataMember(Name = "static-embedding-text")]
            public string? StaticEmbeddingText { set; get; }

            public ExpandConfig ToImmutable() => new(
                    enabled: this.Enabled ?? true,
                    matchFilePatterns: this.MatchFilePattern ?? Array.Empty<string>(),
                    ignoreFilePatterns: this.IgnoreFilePatternRegex?.Select(s => new Regex(s)) ?? Array.Empty<Regex>(),
                    staticEmbeddingText: this.StaticEmbeddingText,
                    metadataExpandingFile: MetadataExpandingFile);
        }
    }
}
