using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace SourceExpander
{
    public class ExpandConfig
    {
        public ExpandConfig()
            : this(
                  true,
                  Array.Empty<string>(),
                  Array.Empty<Regex>(),
                  null,
                  null)
        { }
        public ExpandConfig(
            bool enabled,
            string[] matchFilePatterns,
            IEnumerable<Regex> ignoreFilePatterns,
            string? staticEmbeddingText,
            string? metadataExpandingFile)
        {
            Enabled = enabled;
            MatchFilePatterns = ImmutableArray.Create(matchFilePatterns);
            IgnoreFilePatterns = ImmutableArray.CreateRange(ignoreFilePatterns);
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

        public static ExpandConfig Parse(string sourceText)
        {
            if (JsonUtil.ParseJson<ExpandConfig>(sourceText) is { } config)
                return config;
            return new ExpandConfig();
        }
        static ExpandConfig()
        {
            JsonUtil.Converters.Add(new ExpandConfigConverter());
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
                    ignoreFilePatterns: this.IgnoreFilePatternRegex?.Select(s => new Regex(s))
                    ?? Array.Empty<Regex>(),
                    staticEmbeddingText: this.StaticEmbeddingText,
                    metadataExpandingFile: MetadataExpandingFile);
        }
    }
}
