using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp;

namespace SourceExpander;

internal record EmbeddedData(
    string AssemblyName,
    ImmutableArray<SourceFileInfo> Sources,
    Version EmbedderVersion,
    string CSharpVersion,
    bool AllowUnsafe,
    ImmutableArray<string> EmbeddedNamespaces)
{
    public static EmbeddedData Empty => new("Empty",
        ImmutableArray<SourceFileInfo>.Empty, new(1, 0, 0), LanguageVersion.Default.ToDisplayString(), false, ImmutableArray<string>.Empty);

    public static (EmbeddedData Data, ImmutableArray<(string Key, string ErrorMessage)> Errors)
        Create(string assemblyName, IEnumerable<KeyValuePair<string, string>> assemblyMetadatas)
    {
        var errors = ImmutableArray.CreateBuilder<(string Key, string ErrorMessage)>();
        string csharpVersion = LanguageVersion.CSharp1.ToDisplayString();
        Version version = new(1, 0, 0);
        bool allowUnsafe = false;
        ImmutableArray<string> embeddedNamespaces = ImmutableArray<string>.Empty;

        var builder = ImmutableArray.CreateBuilder<SourceFileInfo>();
        foreach (var pair in assemblyMetadatas)
        {
            var key = pair.Key;
            var value = pair.Value;
            var keyArray = key.Split('.');
            if (keyArray.Length < 2 || keyArray[0] != "SourceExpander")
                continue;

            switch (TryGetSourceFileInfos(keyArray, value))
            {
                case NotMatch<SourceFileInfo[]>:
                    break;
                case Success<SourceFileInfo[]> { Value: var sources }:
                    builder.AddRange(sources);
                    continue;
                case Error<SourceFileInfo[]> { Message: var message }:
                    errors.Add((key, message));
                    continue;
            }
            switch (TryGetEmbedderVersion(keyArray, value))
            {
                case NotMatch<Version>:
                    break;
                case Success<Version> { Value: var attrVersion }:
                    version = attrVersion;
                    continue;
                case Error<Version> { Message: var message }:
                    errors.Add((key, message));
                    continue;
            }
            switch (TryGetEmbeddedLanguageVersion(keyArray, value))
            {
                case NotMatch<string>:
                    break;
                case Success<string> { Value: var attrCSharpVersion }:
                    csharpVersion = attrCSharpVersion;
                    continue;
                case Error<string> { Message: var message }:
                    errors.Add((key, message));
                    continue;
            }
            switch (TryGetEmbeddedAllowUnsafe(keyArray, value))
            {
                case NotMatch<bool>:
                    break;
                case Success<bool> { Value: var attrAllowUnsafe }:
                    allowUnsafe = attrAllowUnsafe;
                    continue;
                case Error<bool> { Message: var message }:
                    errors.Add((key, message));
                    continue;
            }
            switch (TryGetEmbeddedNamespaces(keyArray, value))
            {
                case NotMatch<ImmutableArray<string>>:
                    break;
                case Success<ImmutableArray<string>> { Value: var attrNamespaces }:
                    embeddedNamespaces = attrNamespaces;
                    continue;
                case Error<ImmutableArray<string>> { Message: var message }:
                    errors.Add((key, message));
                    continue;
            }
        }
        return (new EmbeddedData(assemblyName, builder.ToImmutable(), version, csharpVersion, allowUnsafe, embeddedNamespaces), errors.ToImmutable());
    }

    abstract record ParseResult
    {
        public static Success<T> Success<T>(T value) => new(value);
        public static Error Error(string message) => new(message);
        public static readonly NotMatch NotMatch = new();
    }
    abstract record ParseResult<T> : ParseResult
    {
        public static implicit operator ParseResult<T>(Error e) => new Error<T>(e.Message);
        public static implicit operator ParseResult<T>(NotMatch _) => new NotMatch<T>();
    }
    record Success<T>(T Value) : ParseResult<T>;
    record Error(string Message);
    record Error<T>(string Message) : ParseResult<T>;
    record NotMatch;
    record NotMatch<T> : ParseResult<T>;

    private static ParseResult<SourceFileInfo[]> TryGetSourceFileInfos(string[] keyArray, string value)
    {
        try
        {
            if (keyArray.Length >= 2
                && keyArray[1] == "EmbeddedSourceCode")
            {
                var embedded = Array.IndexOf(keyArray, "GZipBase32768", 2) >= 0
                    ? JsonUtil.ParseJson<SourceFileInfo[]>(SourceFileInfoUtil.FromGZipBase32768ToStream(value))
                    : JsonUtil.ParseJson<SourceFileInfo[]>(value);
                if (embedded is { Length: > 0 })
                    return ParseResult.Success(embedded);
            }
            return ParseResult.NotMatch;
        }
        catch (Exception e)
        {
            return ParseResult.Error(e.Message);
        }
    }
    private static ParseResult<Version> TryGetEmbedderVersion(string[] keyArray, string value)
    {
        try
        {
            if (keyArray.Length == 2
                && keyArray[1] == "EmbedderVersion"
                && Version.TryParse(value, out var embedderVersion))
            {
                return ParseResult.Success(embedderVersion);
            }
            return ParseResult.NotMatch;
        }
        catch (Exception e)
        {
            return ParseResult.Error(e.Message);
        }
    }
    private static ParseResult<string> TryGetEmbeddedLanguageVersion(string[] keyArray, string value)
    {
        try
        {
            if (keyArray.Length == 2
                && keyArray[1] == "EmbeddedLanguageVersion")
            {
                return ParseResult.Success(value);
            }
            return ParseResult.NotMatch;
        }
        catch (Exception e)
        {
            return ParseResult.Error(e.Message);
        }
    }
    private static ParseResult<bool> TryGetEmbeddedAllowUnsafe(string[] keyArray, string value)
    {
        try
        {
            if (keyArray.Length == 2
                && keyArray[1] == "EmbeddedAllowUnsafe"
                && bool.TryParse(value, out var embeddedAllowUnsafe))
            {
                return ParseResult.Success(embeddedAllowUnsafe);
            }
            return ParseResult.NotMatch;
        }
        catch (Exception e)
        {
            return ParseResult.Error(e.Message);
        }
    }
    private static ParseResult<ImmutableArray<string>> TryGetEmbeddedNamespaces(string[] keyArray, string value)
    {
        try
        {
            if (keyArray.Length == 2
                && keyArray[1] == "EmbeddedNamespaces")
            {
                return ParseResult.Success(ImmutableArray.Create(value.Split(',')));
            }
            return ParseResult.NotMatch;
        }
        catch (Exception e)
        {
            return ParseResult.Error(e.Message);
        }
    }
}
