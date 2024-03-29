﻿using System;
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
                    if (analyzerConfigOptions.TryGetValue(header + nameof(data.Enabled), out string? v) && !string.IsNullOrWhiteSpace(v))
                        data.Enabled = !StringComparer.OrdinalIgnoreCase.Equals(v, "false");
                    if (analyzerConfigOptions.TryGetValue(header + nameof(data.MatchFilePattern), out v) && !string.IsNullOrWhiteSpace(v))
                        data.MatchFilePattern = v.Split(';').Select(t => t.Trim()).ToArray();
                    if (analyzerConfigOptions.TryGetValue(header + nameof(data.MetadataExpandingFile), out v) && !string.IsNullOrWhiteSpace(v))
                        data.MetadataExpandingFile = v;
                    if (analyzerConfigOptions.TryGetValue(header + nameof(data.IgnoreFilePatternRegex), out v) && !string.IsNullOrWhiteSpace(v))
                        data.IgnoreFilePatternRegex = [v];
                    if (analyzerConfigOptions.TryGetValue(header + nameof(data.IgnoreAssemblies), out v) && !string.IsNullOrWhiteSpace(v))
                        data.IgnoreAssemblies = v.Split(';').Select(t => t.Trim()).ToArray();
                    if (analyzerConfigOptions.TryGetValue(header + nameof(data.StaticEmbeddingText), out v) && !string.IsNullOrWhiteSpace(v))
                        data.StaticEmbeddingText = v;
                    if (analyzerConfigOptions.TryGetValue(header + nameof(data.ExpandingAll), out v) && !string.IsNullOrWhiteSpace(v))
                        data.ExpandingAll = !StringComparer.OrdinalIgnoreCase.Equals(v, "false");
                    if (analyzerConfigOptions.TryGetValue(header + nameof(data.ExpandingByGroup), out v) && !string.IsNullOrWhiteSpace(v))
                        data.ExpandingByGroup = !StringComparer.OrdinalIgnoreCase.Equals(v, "false");
                    if (analyzerConfigOptions.TryGetValue(header + nameof(data.ExpandingPosition), out v) && !string.IsNullOrWhiteSpace(v))
                        data.ExpandingPosition = v;
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
