using System;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SourceExpander
{
    internal partial record EmbedderConfig
    {
        public static EmbedderConfig Parse(string? sourceText, AnalyzerConfigOptions analyzerConfigOptions)
        {
            try
            {
                var data = sourceText switch
                {
                    { } => JsonUtil.ParseJson<EmbedderConfigData>(sourceText) ?? new(),
                    _ => new(),
                };
                {
                    const string buildPropHeader = "build_property.";
                    const string header = buildPropHeader + "SourceExpander_Embedder_";
                    if (analyzerConfigOptions.TryGetValue(header + nameof(data.Enabled), out string? v) && !string.IsNullOrWhiteSpace(v))
                        data.Enabled = !StringComparer.OrdinalIgnoreCase.Equals(v, "false");
                    if (analyzerConfigOptions.TryGetValue(header + nameof(data.Include), out v) && !string.IsNullOrWhiteSpace(v))
                        data.Include = v.Split(';').Select(t => t.Trim()).ToArray();
                    if (analyzerConfigOptions.TryGetValue(header + nameof(data.Exclude), out v) && !string.IsNullOrWhiteSpace(v))
                        data.Exclude = v.Split(';').Select(t => t.Trim()).ToArray();
                    if (analyzerConfigOptions.TryGetValue(header + nameof(data.EmbeddingType), out v) && !string.IsNullOrWhiteSpace(v))
                        data.EmbeddingType = v;
                    if (analyzerConfigOptions.TryGetValue(header + nameof(data.ExcludeAttributes), out v) && !string.IsNullOrWhiteSpace(v))
                        data.ExcludeAttributes = v.Split(';').Select(t => t.Trim()).ToArray();
                    if (analyzerConfigOptions.TryGetValue(header + nameof(data.RemoveConditional), out v) && !string.IsNullOrWhiteSpace(v))
                        data.RemoveConditional = v.Split(';').Select(t => t.Trim()).ToArray();
                    if (analyzerConfigOptions.TryGetValue(header + nameof(data.MinifyLevel), out v) && !string.IsNullOrWhiteSpace(v))
                        data.MinifyLevel = v;
                    if (analyzerConfigOptions.TryGetValue(header + nameof(data.EmbeddingFileNameType), out v) && !string.IsNullOrWhiteSpace(v))
                        data.EmbeddingFileNameType = v;
                    if (analyzerConfigOptions.TryGetValue(header + nameof(data.ExpandingSymbol), out v) && !string.IsNullOrWhiteSpace(v))
                        data.ExpandingSymbol = v;
#pragma warning disable CS0612
                    if (analyzerConfigOptions.TryGetValue(header + nameof(data.EnableMinify), out v) && !string.IsNullOrWhiteSpace(v))
                        data.EnableMinify = !StringComparer.OrdinalIgnoreCase.Equals(v, "false");
#pragma warning restore CS0612
                }
                return data.ToImmutable();
            }
            catch (Exception e)
            {
                throw new ParseJsonException(e);
            }
        }

        [DataContract]
        private class EmbedderConfigData
        {
            [DataMember(Name = "enabled")]
            public bool? Enabled { set; get; }
            [DataMember(Name = "include")]
            public string[]? Include { set; get; }
            [DataMember(Name = "exclude")]
            public string[]? Exclude { set; get; }
            [DataMember(Name = "embedding-type")]
            public string? EmbeddingType { set; get; }
            [DataMember(Name = "exclude-attributes")]
            public string[]? ExcludeAttributes { set; get; }
            [DataMember(Name = "minify-level")]
            public string? MinifyLevel { set; get; }
            [DataMember(Name = "remove-conditional")]
            public string[]? RemoveConditional { set; get; }
            [DataMember(Name = "embedding-source-class-name")]
            public string? EmbeddingSourceClassName { set; get; }
            [DataMember(Name = "embedding-filename-type")]
            public string? EmbeddingFileNameType { set; get; }
            [DataMember(Name = "expanding-symbol")]
            public string? ExpandingSymbol { set; get; }

            [Obsolete]
            [DataMember(Name = "embedding-source-class")]
            public object? EmbeddingSourceClass { set; get; }
            [Obsolete]
            [DataMember(Name = "enable-minify")]
            public bool? EnableMinify { set; get; }

            private EmbeddingType ParsedEmbeddingType
                => Enum.TryParse(EmbeddingType, true, out EmbeddingType r) ? r : SourceExpander.EmbeddingType.GZipBase32768;
            private MinifyLevel ParsedMinifyLevel
                => Enum.TryParse(MinifyLevel, true, out MinifyLevel r) ? r : SourceExpander.MinifyLevel.Default;
            private EmbeddingFileNameType ParsedEmbeddingFileNameType
                => Enum.TryParse(EmbeddingFileNameType, true, out EmbeddingFileNameType r) ? r : SourceExpander.EmbeddingFileNameType.WithoutCommonPrefix;

            public EmbedderConfig ToImmutable() =>
                new(
                        Enabled ?? true,
                        Include,
                        Exclude,
                        ParsedEmbeddingType,
                        ExcludeAttributes,
                        ParsedMinifyLevel,
                        RemoveConditional,
                        EmbeddingSourceClassName,
                        ParsedEmbeddingFileNameType,
                        expandingSymbol: ExpandingSymbol,
                        obsoleteConfigProperties: GetObsoleteConfigProperties());

            private ImmutableArray<ObsoleteConfigProperty> GetObsoleteConfigProperties()
            {
                var builder = ImmutableArray.CreateBuilder<ObsoleteConfigProperty>();
#pragma warning disable CS0612
                if (EnableMinify.HasValue)
                    builder.Add(ObsoleteConfigProperty.EnableMinify);
                if (EmbeddingSourceClass != null)
                    builder.Add(ObsoleteConfigProperty.EmbeddingSourceClass);
#pragma warning restore CS0612
                return builder.ToImmutable();
            }
        }
    }
}
