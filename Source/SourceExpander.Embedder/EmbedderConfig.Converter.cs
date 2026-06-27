using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp;

namespace SourceExpander;

internal partial record EmbedderConfig
{
    private class EmbedderConfigData
    {
        [JsonProperty("enabled")]
        public bool? Enabled { set; get; }
        [JsonProperty("include")]
        public string[]? Include { set; get; }
        [JsonProperty("exclude")]
        public string[]? Exclude { set; get; }
        [JsonProperty("embedding-type")]
        public string? EmbeddingType { set; get; }
        [JsonProperty("exclude-attributes")]
        public string[]? ExcludeAttributes { set; get; }
        [JsonProperty("minify-level")]
        public string? MinifyLevel { set; get; }
        [JsonProperty("remove-conditional")]
        public string[]? RemoveConditional { set; get; }
        [JsonProperty("embedding-source-class-name")]
        public string? EmbeddingSourceClassName { set; get; }
        [JsonProperty("embedding-filename-type")]
        public string? EmbeddingFileNameType { set; get; }

        [JsonProperty("language-version")]
        public object? LanguageVersion { set; get; }

        [JsonProperty("expand-in-library")]
        public bool? ExpandInLibrary { set; get; }

        [Obsolete]
        [JsonProperty("expanding-symbol")]
        public string? ExpandingSymbol { set; get; }
        [Obsolete]
        [JsonProperty("embedding-source-class")]
        public object? EmbeddingSourceClass { set; get; }
        [Obsolete]
        [JsonProperty("enable-minify")]
        public bool? EnableMinify { set; get; }

        private EmbeddingType ParsedEmbeddingType
            => Enum.TryParse(EmbeddingType, true, out EmbeddingType r) ? r : SourceExpander.EmbeddingType.GZipBase32768;
        private MinifyLevel ParsedMinifyLevel
            => Enum.TryParse(MinifyLevel, true, out MinifyLevel r) ? r : SourceExpander.MinifyLevel.Default;
        private EmbeddingFileNameType ParsedEmbeddingFileNameType
            => Enum.TryParse(EmbeddingFileNameType, true, out EmbeddingFileNameType r) ? r : SourceExpander.EmbeddingFileNameType.WithoutCommonPrefix;

        private LanguageVersion? ParsedLanguageVersion
            => LanguageVersion?.ToString() switch
            {
                { } version when LanguageVersionFacts.TryParse(version, out var result) => result.MapSpecifiedToEffectiveVersion(),
                _ => null,
            };

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
                    languageVersion: ParsedLanguageVersion,
                    expandInLibrary: ExpandInLibrary,
                    obsoleteConfigProperties: GetObsoleteConfigProperties());

        private ImmutableArray<ObsoleteConfigProperty> GetObsoleteConfigProperties()
        {
            var builder = ImmutableArray.CreateBuilder<ObsoleteConfigProperty>();
#pragma warning disable CS0612
            if (EnableMinify.HasValue)
                builder.Add(ObsoleteConfigProperty.EnableMinify);
            if (EmbeddingSourceClass != null)
                builder.Add(ObsoleteConfigProperty.EmbeddingSourceClass);
            if (ExpandingSymbol != null)
                builder.Add(ObsoleteConfigProperty.ExpandingSymbol);
#pragma warning restore CS0612
            return builder.ToImmutable();
        }
    }
}
