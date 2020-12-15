using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.Serialization;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace SourceExpander
{
    public class EmbedderConfig
    {
        public EmbedderConfig()
            : this(
                  true,
                  EmbeddingType.GZipBase32768,
                  Array.Empty<string>(),
                  ImmutableHashSet<SyntaxKind>.Empty)
        { }
        public EmbedderConfig(
            bool enabled,
            EmbeddingType embeddingType,
            string[] excludeAttributes,
            IEnumerable<SyntaxKind> addTriviaKinds)
        {
            Enabled = enabled;
            EmbeddingType = embeddingType;
            ExcludeAttributes = ImmutableHashSet.Create(excludeAttributes);
            AddTriviaKinds = addTriviaKinds switch
            {
                ImmutableHashSet<SyntaxKind> hs => hs,
                ImmutableHashSet<SyntaxKind>.Builder hs => hs.ToImmutable(),
                _ => ImmutableHashSet.CreateRange(addTriviaKinds),
            };
        }

        public bool Enabled { get; }
        public EmbeddingType EmbeddingType { get; }
        public ImmutableHashSet<string> ExcludeAttributes { get; }
        public ImmutableHashSet<SyntaxKind> AddTriviaKinds { get; }

        public static EmbedderConfig Parse(SourceText? sourceText, CancellationToken cancellationToken)
        {
            static EmbeddingType ParseEmbeddingType(string? str)
                => str?.ToLowerInvariant() switch
                {
                    "raw" => EmbeddingType.Raw,
                    _ => EmbeddingType.GZipBase32768,
                };

            static ImmutableHashSet<SyntaxKind> ParseSyntaxKinds(string[]? arr)
            {
                if (arr is null)
                    return ImmutableHashSet<SyntaxKind>.Empty;

                var builder = ImmutableHashSet.CreateBuilder<SyntaxKind>();
                foreach (var str in arr)
                    if (Enum.TryParse(str, true, out SyntaxKind kind))
                        builder.Add(kind);
                return builder.ToImmutable();
            }

            try
            {
                if (sourceText is not null && JsonUtil.ParseJson<EmbedderConfigData>(sourceText, cancellationToken) is { } data)
                    return new EmbedderConfig(
                        enabled: data.Enabled ?? true,
                        embeddingType: ParseEmbeddingType(data.EmbeddingType),
                        excludeAttributes: data.ExcludeAttributes ?? Array.Empty<string>(),
                        addTriviaKinds: ParseSyntaxKinds(data.AddTriviaKinds));
                return new EmbedderConfig();
            }
            catch (Exception e)
            {
                throw new ParseConfigException(e);
            }
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
            [DataMember(Name = "add-trivia-kinds")]
            public string[]? AddTriviaKinds { set; get; }

            public ExtensionDataObject? ExtensionData { get; set; }
        }
    }

    public enum EmbeddingType
    {
        GZipBase32768,
        Raw,
    }

#pragma warning disable CA1032
    internal sealed class ParseConfigException : Exception
    {
        public ParseConfigException() { }
        public ParseConfigException(Exception inner) : base(inner.Message, inner) { }
    }
}
