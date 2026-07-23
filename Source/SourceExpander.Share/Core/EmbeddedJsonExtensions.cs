using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;

namespace SourceExpander;

internal static class EmbeddedJsonExtensions
{
    private const string EMBEDDED_FILE_NAME = "SourceExpander.Embedded.json";

    private static bool IsEmbeddedJson(AdditionalText a) => a.Path.EndsWith(EMBEDDED_FILE_NAME, StringComparison.OrdinalIgnoreCase);

    extension(IncrementalGeneratorInitializationContext context)
    {
        public IncrementalValueProvider<ImmutableArray<AdditionalText>> EmbeddedJsonProvider
            => context.AdditionalTextsProvider
            .Where(IsEmbeddedJson)
            .Collect();
    }

    extension(ImmutableArray<AdditionalText> additionalTexts)
    {
        public (ImmutableArray<EmbeddedData> Result, ImmutableArray<AdditionalText> Error) ToEmbeddData(CancellationToken cancellationToken)
            => additionalTexts.ToEmbeddData(false, cancellationToken);

        public (ImmutableArray<EmbeddedData> Result, ImmutableArray<AdditionalText> Error) ToEmbeddData(bool filter, CancellationToken cancellationToken)
        {
            var embeddedDataBuilder = ImmutableArray.CreateBuilder<EmbeddedData>();
            var errorBuilder = ImmutableArray.CreateBuilder<AdditionalText>();
            foreach (var additionalText in additionalTexts)
            {
                if (filter && !IsEmbeddedJson(additionalText)) continue;
                if (additionalText.GetText(cancellationToken) is { } sourceText)
                {
                    try
                    {
                        var obj = JsonUtil.ParseJson<EmbeddedData>(sourceText);
                        if (obj is not null)
                            embeddedDataBuilder.Add(obj);
                    }
                    catch (ParseJsonException)
                    {
                        errorBuilder.Add(additionalText);
                    }
                }
            }
            return (embeddedDataBuilder.ToImmutable(), errorBuilder.ToImmutable());
        }
    }
}
