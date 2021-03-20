using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis.Text;

namespace SourceExpander
{
    public class ExpandConfig
    {
        public ExpandConfig()
            : this(
                  true,
                  Array.Empty<string>(),
                  Array.Empty<Regex>(),
                  null)
        { }
        public ExpandConfig(
            bool enabled,
            string[] matchFilePatterns,
            IEnumerable<Regex> ignoreFilePatterns,
            string? staticEmbeddingText)
        {
            Enabled = enabled;
            MatchFilePatterns = ImmutableArray.Create(matchFilePatterns);
            IgnoreFilePatterns = ImmutableArray.CreateRange(ignoreFilePatterns);
            StaticEmbeddingText = staticEmbeddingText;
        }

        public bool Enabled { get; }
        public ImmutableArray<string> MatchFilePatterns { get; }
        public ImmutableArray<Regex> IgnoreFilePatterns { get; }
        public string? StaticEmbeddingText { get; }
        public bool IsMatch(string filePath)
            => (MatchFilePatterns.Length == 0
                || MatchFilePatterns.Any(p => filePath.IndexOf(p, StringComparison.OrdinalIgnoreCase) >= 0))
                && IgnoreFilePatterns.All(regex => !regex.IsMatch(filePath));

        public static ExpandConfig Parse(SourceText? sourceText)
        {
            try
            {
                if (sourceText is not null && JsonUtil.ParseJson<ExpandConfigData>(sourceText) is { } data)
                    return new ExpandConfig(
                        enabled: data.Enabled ?? true,
                        matchFilePatterns: data.MatchFilePattern ?? Array.Empty<string>(),
                        ignoreFilePatterns: data.IgnoreFilePatternRegex?.Select(s => new Regex(s))
                        ?? Array.Empty<Regex>(),
                        staticEmbeddingText: data.StaticEmbeddingText);
                return new ExpandConfig();
            }
            catch (Exception e)
            {
                throw new ParseConfigException(e);
            }
        }

        [DataContract]
        private class ExpandConfigData
        {
            [DataMember(Name = "enabled")]
            public bool? Enabled { set; get; }
            [DataMember(Name = "match-file-pattern")]
            public string[]? MatchFilePattern { set; get; }
            [DataMember(Name = "ignore-file-pattern-regex")]
            public string[]? IgnoreFilePatternRegex { set; get; }
            [DataMember(Name = "static-embedding-text")]
            public string? StaticEmbeddingText { set; get; }
        }
    }

#pragma warning disable CA1032
    internal sealed class ParseConfigException : Exception
    {
        public ParseConfigException() { }
        public ParseConfigException(Exception inner) : base(inner.Message, inner) { }
    }
}
