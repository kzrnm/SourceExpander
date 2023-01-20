using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SourceExpander
{
    public partial record ExpandConfig
    {
        public static ExpandConfig Parse(string? sourceText, AnalyzerConfigOptions analyzerConfigOptions)
        {
            try
            {
                var data = sourceText switch
                {
                    { } => JsonUtil.ParseJson<ExpandConfigData>(sourceText) ?? new(),
                    _ => new(),
                };
                {
                    const string buildPropHeader = "build_property.";
                    const string header = buildPropHeader + "SourceExpander_Generator_";
                    if (analyzerConfigOptions.TryGetValue(header + "Enabled", out string? v) && !string.IsNullOrWhiteSpace(v))
                        data.Enabled = !StringComparer.OrdinalIgnoreCase.Equals(v, "false");
                    if (analyzerConfigOptions.TryGetValue(header + "MatchFilePattern", out v) && !string.IsNullOrWhiteSpace(v))
                        data.MatchFilePattern = v.Split(';').Select(t => t.Trim()).ToArray();
                    if (analyzerConfigOptions.TryGetValue(header + "MetadataExpandingFile", out v) && !string.IsNullOrWhiteSpace(v))
                        data.MetadataExpandingFile = v;
                    if (analyzerConfigOptions.TryGetValue(header + "IgnoreFilePatternRegex", out v) && !string.IsNullOrWhiteSpace(v))
                        data.IgnoreFilePatternRegex = new[] { v };
                    if (analyzerConfigOptions.TryGetValue(header + "IgnoreAssemblies", out v) && !string.IsNullOrWhiteSpace(v))
                        data.IgnoreAssemblies = v.Split(';').Select(t => t.Trim()).ToArray();
                    if (analyzerConfigOptions.TryGetValue(header + "StaticEmbeddingText", out v) && !string.IsNullOrWhiteSpace(v))
                        data.StaticEmbeddingText = v;
                }
                return data.ToImmutable();
            }
            catch (Exception e)
            {
                throw new ParseJsonException(e);
            }
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
            [DataMember(Name = "ignore-assemblies")]
            public string[]? IgnoreAssemblies { set; get; }
            [DataMember(Name = "static-embedding-text")]
            public string? StaticEmbeddingText { set; get; }

            public ExpandConfig ToImmutable() => new(
                    enabled: this.Enabled ?? true,
                    matchFilePatterns: this.MatchFilePattern ?? Array.Empty<string>(),
                    ignoreAssemblies: this.IgnoreAssemblies ?? Array.Empty<string>(),
                    ignoreFilePatterns: this.IgnoreFilePatternRegex?.Select(s => new Regex(s)) ?? Array.Empty<Regex>(),
                    staticEmbeddingText: this.StaticEmbeddingText,
                    metadataExpandingFile: MetadataExpandingFile);
        }
    }
}
