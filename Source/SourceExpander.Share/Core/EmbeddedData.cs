using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp;

namespace SourceExpander
{
    internal class EmbeddedData
    {
        public string AssemblyName { get; }
        public Version EmbedderVersion { get; }
        public LanguageVersion CSharpVersion { get; }
        public ImmutableArray<SourceFileInfo> Sources { get; }
        public bool AllowUnsafe { get; }
        public ImmutableArray<string> EmbeddedNamespaces { get; }
        public bool IsEmpty => Sources.Length == 0;
        internal EmbeddedData(string assemblyName, ImmutableArray<SourceFileInfo> sources,
            Version embedderVersion,
            LanguageVersion csharpVersion,
            bool allowUnsafe,
            ImmutableArray<string> embeddedNamespaces)
        {
            AssemblyName = assemblyName;
            Sources = sources;
            EmbedderVersion = embedderVersion;
            CSharpVersion = csharpVersion;
            AllowUnsafe = allowUnsafe;
            EmbeddedNamespaces = embeddedNamespaces;
        }
        public static EmbeddedData Empty => new("Empty",
            ImmutableArray<SourceFileInfo>.Empty,
            new(1, 0, 0),
            LanguageVersion.Default,
            false,
            ImmutableArray<string>.Empty);
        public static (EmbeddedData Data, ImmutableArray<(string Key, string ErrorMessage)> Errors)
            Create(string assemblyName, IEnumerable<KeyValuePair<string, string>> assemblyMetadatas)
        {
            var errors = ImmutableArray.CreateBuilder<(string Key, string ErrorMessage)>();
            LanguageVersion csharpVersion = LanguageVersion.CSharp1;
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

                ParseResult r;
                switch ((r = TryAddSourceFileInfos(keyArray, value, builder)).Result)
                {
                    case ParseResult.Status.NotMatch:
                        break;
                    case ParseResult.Status.Success:
                        continue;
                    case ParseResult.Status.Error:
                        errors.Add((key, r.Message));
                        continue;
                }
                switch ((r = TryGetEmbedderVersion(keyArray, value, out var attrVersion)).Result)
                {
                    case ParseResult.Status.NotMatch:
                        break;
                    case ParseResult.Status.Success:
                        version = attrVersion!;
                        continue;
                    case ParseResult.Status.Error:
                        errors.Add((key, r.Message));
                        continue;
                }
                switch ((r = TryGetEmbeddedLanguageVersion(keyArray, value, out var attrCSharpVersion)).Result)
                {
                    case ParseResult.Status.NotMatch:
                        break;
                    case ParseResult.Status.Success:
                        csharpVersion = attrCSharpVersion;
                        continue;
                    case ParseResult.Status.Error:
                        errors.Add((key, r.Message));
                        continue;
                }
                switch ((r = TryGetEmbeddedAllowUnsafe(keyArray, value, out var attrAllowUnsafe)).Result)
                {
                    case ParseResult.Status.NotMatch:
                        break;
                    case ParseResult.Status.Success:
                        allowUnsafe = attrAllowUnsafe;
                        continue;
                    case ParseResult.Status.Error:
                        errors.Add((key, r.Message));
                        continue;
                }
                switch ((r = TryGetEmbeddedNamespaces(keyArray, value, out var attrNamespaces)).Result)
                {
                    case ParseResult.Status.NotMatch:
                        break;
                    case ParseResult.Status.Success:
                        embeddedNamespaces = attrNamespaces.ToImmutableArray();
                        continue;
                    case ParseResult.Status.Error:
                        errors.Add((key, r.Message));
                        continue;
                }
            }
            return (new EmbeddedData(assemblyName, builder.ToImmutable(), version, csharpVersion, allowUnsafe, embeddedNamespaces), errors.ToImmutable());
        }
        private readonly struct ParseResult
        {
            private ParseResult(Status result, string message)
            {
                Result = result;
                Message = message;
            }

            public static readonly ParseResult Success = new(Status.Success, string.Empty);
            public static readonly ParseResult NotMatch = new(Status.NotMatch, string.Empty);
            public static ParseResult Error(string message) => new(Status.Error, message);
            public enum Status
            {
                Success,
                NotMatch,
                Error,
            }
            public Status Result { get; }
            public string Message { get; }
        }
        private static ParseResult TryAddSourceFileInfos(string[] keyArray, string value, ImmutableArray<SourceFileInfo>.Builder builder)
        {
            try
            {
                if (keyArray.Length >= 2
                    && keyArray[1] == "EmbeddedSourceCode")
                {
                    ImmutableArray<SourceFileInfo> embedded;
                    if (Array.IndexOf(keyArray, "GZipBase32768", 2) >= 0)
                        embedded = ImmutableArray.Create(JsonUtil.ParseJson<SourceFileInfo[]>(SourceFileInfoUtil.FromGZipBase32768ToStream(value)));
                    else
                        embedded = ImmutableArray.Create(JsonUtil.ParseJson<SourceFileInfo[]>(value));
                    builder.AddRange(embedded);
                    return ParseResult.Success;
                }
                return ParseResult.NotMatch;
            }
            catch (Exception e)
            {
                return ParseResult.Error(e.Message);
            }
        }
        private static ParseResult TryGetEmbedderVersion(string[] keyArray, string value, out Version? version)
        {
            version = null;
            try
            {
                if (keyArray.Length == 2
                    && keyArray[1] == "EmbedderVersion"
                    && Version.TryParse(value, out var embedderVersion))
                {
                    version = embedderVersion;
                    return ParseResult.Success;
                }
                return ParseResult.NotMatch;
            }
            catch (Exception e)
            {
                return ParseResult.Error(e.Message);
            }
        }
        private static ParseResult TryGetEmbeddedLanguageVersion(string[] keyArray, string value, out LanguageVersion version)
        {
            version = LanguageVersion.Default;
            try
            {
                if (keyArray.Length == 2
                    && keyArray[1] == "EmbeddedLanguageVersion"
                    && LanguageVersionFacts.TryParse(value, out var embeddedVersion))
                {
                    version = embeddedVersion;
                    return ParseResult.Success;
                }
                return ParseResult.NotMatch;
            }
            catch (Exception e)
            {
                return ParseResult.Error(e.Message);
            }
        }
        private static ParseResult TryGetEmbeddedAllowUnsafe(string[] keyArray, string value, out bool allowUnsafe)
        {
            allowUnsafe = false;
            try
            {
                if (keyArray.Length == 2
                    && keyArray[1] == "EmbeddedAllowUnsafe"
                    && bool.TryParse(value, out var embeddedAllowUnsafe))
                {
                    allowUnsafe = embeddedAllowUnsafe;
                    return ParseResult.Success;
                }
                return ParseResult.NotMatch;
            }
            catch (Exception e)
            {
                return ParseResult.Error(e.Message);
            }
        }
        private static ParseResult TryGetEmbeddedNamespaces(string[] keyArray, string value, out string[] embeddedNamespaces)
        {
            embeddedNamespaces = Array.Empty<string>();
            try
            {
                if (keyArray.Length == 2
                    && keyArray[1] == "EmbeddedNamespaces")
                {
                    embeddedNamespaces = value.Split(',');
                    return ParseResult.Success;
                }
                return ParseResult.NotMatch;
            }
            catch (Exception e)
            {
                return ParseResult.Error(e.Message);
            }
        }
    }
}
