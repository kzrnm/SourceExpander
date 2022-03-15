using System;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SourceExpander
{
    internal partial class EmbedderConfig
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
                    if (analyzerConfigOptions.TryGetValue(header + "Enabled", out string? v) && !string.IsNullOrWhiteSpace(v))
                        data.Enabled = !StringComparer.OrdinalIgnoreCase.Equals(v, "false");
                    if (analyzerConfigOptions.TryGetValue(header + "EmbeddingType", out v) && !string.IsNullOrWhiteSpace(v))
                        data.EmbeddingType = v;
                    if (analyzerConfigOptions.TryGetValue(header + "ExcludeAttributes", out v) && !string.IsNullOrWhiteSpace(v))
                        data.ExcludeAttributes = v.Split(';').Select(t => t.Trim()).ToArray();
                    if (analyzerConfigOptions.TryGetValue(header + "RemoveConditional", out v) && !string.IsNullOrWhiteSpace(v))
                        data.RemoveConditional = v.Split(';').Select(t => t.Trim()).ToArray();
                    if (analyzerConfigOptions.TryGetValue(header + "MinifyLevel", out v) && !string.IsNullOrWhiteSpace(v))
                        data.MinifyLevel = v;
                    if (analyzerConfigOptions.TryGetValue(header + "EmbeddingFileNameType", out v) && !string.IsNullOrWhiteSpace(v))
                        data.EmbeddingFileNameType = v;

                    if (analyzerConfigOptions.TryGetValue(buildPropHeader + "ProjectDir", out v) && !string.IsNullOrWhiteSpace(v))
                        data.ProjectDir = v;
                }
                return data.ToImmutable();
            }
            catch (Exception e)
            {
                throw new ParseJsonException(e);
            }
        }

        [DataContract]
        private class SourceClassData
        {
            [DataMember(Name = "enabled")]
            public bool? Enabled { set; get; }
            [DataMember(Name = "class-name")]
            public string? ClassName { set; get; }
            public EmbeddingSourceClass ToImmutable() => new(Enabled == true, ClassName);
        }

        [DataContract]
        private class EmbedderConfigData
        {
            [DataMember(Name = "enabled")]
            public bool? Enabled { set; get; }
            [DataMember(Name = "embedding-type")]
            public string? EmbeddingType { set; get; }
            [DataMember(Name = "exclude-attributes")]
            public string[]? ExcludeAttributes { set; get; }
            [DataMember(Name = "minify-level")]
            public string? MinifyLevel { set; get; }
            [DataMember(Name = "remove-conditional")]
            public string[]? RemoveConditional { set; get; }
            [DataMember(Name = "embedding-source-class")]
            public SourceClassData? EmbeddingSourceClass { set; get; }
            [DataMember(Name = "embedding-filename-type")]
            public string? EmbeddingFileNameType { set; get; }

            [Obsolete]
            [DataMember(Name = "enable-minify")]
            public bool? EnableMinify { set; get; }


            [IgnoreDataMember]
            public string? ProjectDir { set; get; }

            private EmbeddingType ParsedEmbeddingType
                => Enum.TryParse(EmbeddingType, true, out EmbeddingType r) ? r : SourceExpander.EmbeddingType.GZipBase32768;
            private MinifyLevel ParsedMinifyLevel
                => Enum.TryParse(MinifyLevel, true, out MinifyLevel r) ? r : SourceExpander.MinifyLevel.Default;
            private EmbeddingFileNameType ParsedEmbeddingFileNameType
                => Enum.TryParse(EmbeddingFileNameType, true, out EmbeddingFileNameType r) ? r : SourceExpander.EmbeddingFileNameType.WithoutCommonPrefix;

            public EmbedderConfig ToImmutable() =>
                new(
                        Enabled ?? true,
                        ParsedEmbeddingType,
                        ExcludeAttributes,
                        ParsedMinifyLevel,
                        RemoveConditional,
                        EmbeddingSourceClass?.ToImmutable(),
                        ParsedEmbeddingFileNameType,
                        ProjectDir,
                        GetObsoleteConfigProperties());

            private ImmutableArray<ObsoleteConfigProperty> GetObsoleteConfigProperties()
            {
                var builder = ImmutableArray.CreateBuilder<ObsoleteConfigProperty>();
#pragma warning disable CS0612
                if (EnableMinify.HasValue)
                    builder.Add(ObsoleteConfigProperty.EnableMinify);
#pragma warning restore CS0612
                return builder.ToImmutable();
            }
        }
    }
}
