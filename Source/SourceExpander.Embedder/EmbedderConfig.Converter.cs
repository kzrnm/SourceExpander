using System;
using System.Collections.Immutable;
using System.Runtime.Serialization;

namespace SourceExpander
{
    internal partial record EmbedderConfig
    {
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

            [DataMember(Name = "expand-in-library")]
            public bool? ExpandInLibrary { set; get; }

            [Obsolete]
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
}
