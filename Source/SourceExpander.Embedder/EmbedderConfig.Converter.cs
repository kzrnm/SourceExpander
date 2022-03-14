using System;
using System.Collections.Immutable;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace SourceExpander
{
    [JsonConverter(typeof(EmbedderConfigConverter))]
    internal partial class EmbedderConfig
    {
        private class EmbedderConfigConverter : JsonConverter<EmbedderConfig?>
        {
            public override bool CanWrite => false;
            public override EmbedderConfig? ReadJson(JsonReader reader, Type objectType, EmbedderConfig? existingValue, bool hasExistingValue, JsonSerializer serializer)
                => serializer.Deserialize<EmbedderConfigData>(reader)?.ToImmutable();
            public override void WriteJson(JsonWriter writer, EmbedderConfig? value, JsonSerializer serializer)
                => throw new NotImplementedException("CanWrite is always false");
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

            [DataMember(Name = "enable-minify")]
            public bool? EnableMinify { set; get; }

            static EmbeddingType ParseEmbeddingType(string? str)
                => str?.ToLowerInvariant() switch
                {
                    "raw" => SourceExpander.EmbeddingType.Raw,
                    _ => SourceExpander.EmbeddingType.GZipBase32768,
                };

            static MinifyLevel ParseMinifyLevel(string? str)
                => str?.ToLowerInvariant() switch
                {
                    "full" => SourceExpander.MinifyLevel.Full,
                    "off" => SourceExpander.MinifyLevel.Off,
                    _ => SourceExpander.MinifyLevel.Default,
                };

            public EmbedderConfig ToImmutable() =>
                new(
                        Enabled ?? true,
                        ParseEmbeddingType(EmbeddingType),
                        ExcludeAttributes,
                        ParseMinifyLevel(MinifyLevel),
                        RemoveConditional,
                        EmbeddingSourceClass?.ToImmutable(),
                        GetObsoleteConfigProperties());

            private ImmutableArray<ObsoleteConfigProperty> GetObsoleteConfigProperties()
            {
                var builder = ImmutableArray.CreateBuilder<ObsoleteConfigProperty>();
                if (EnableMinify.HasValue)
                    builder.Add(ObsoleteConfigProperty.EnableMinify);
                return builder.ToImmutable();
            }
        }
    }
}
