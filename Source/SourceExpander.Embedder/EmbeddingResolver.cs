using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SourceExpander.Roslyn;

namespace SourceExpander;

internal class EmbeddingResolver
{
    private CSharpCompilation compilation;
    private readonly CSharpParseOptions parseOptions;
    private readonly IDiagnosticReporter reporter;
    private readonly EmbedderConfig config;
    private readonly bool ConcurrentBuild;
    private readonly CancellationToken cancellationToken;

    public EmbeddingResolver(
        CSharpCompilation compilation,
        CSharpParseOptions parseOptions,
        IDiagnosticReporter reporter,
        EmbedderConfig config,
        CancellationToken cancellationToken = default)
    {
        var specificDiagnosticOptions = new Dictionary<string, ReportDiagnostic>
        {
            { "CS8019", ReportDiagnostic.Warn },
            { "CS0105", ReportDiagnostic.Warn },
        };
        var opts = compilation.Options
            .WithSpecificDiagnosticOptions(specificDiagnosticOptions);
        this.ConcurrentBuild = opts.ConcurrentBuild;
        this.parseOptions = parseOptions.WithDocumentationMode(DocumentationMode.Diagnose);
        this.compilation = compilation.WithOptions(opts);
        this.reporter = reporter;
        this.config = config;
        this.cancellationToken = cancellationToken;
    }
    private struct EmbeddingAssemblyMetadata
    {
        public bool IsEnabled;
        public EmbeddingType EmbeddingType;
        public EmbeddedData EmbeddedData;
        public readonly IEnumerable<(string Key, string Value)> EnumerateMetadatas()
        {
            if (!IsEnabled) yield break;

            var data = EmbeddedData;

            if (EmbeddingType is EmbeddingType.SingleMetadataJson)
            {
                yield return ("SourceExpander.EmbeddedDataJson", JsonUtil.ToJson(data));
                yield break;
            }

            if (data.AllowUnsafe)
                yield return ("SourceExpander.EmbeddedAllowUnsafe", "true");

            yield return ("SourceExpander.EmbedderVersion", data.EmbedderVersion.ToString());
            yield return ("SourceExpander.EmbeddedLanguageVersion", data.CSharpVersion);

            yield return ("SourceExpander.EmbeddedNamespaces", string.Join(",", data.EmbeddedNamespaces));

            var sourceJson = JsonUtil.ToJson(data.Sources);
            yield return EmbeddingType switch
            {
                EmbeddingType.GZipBase32768 => ("SourceExpander.EmbeddedSourceCode.GZipBase32768", SourceFileInfoUtil.ToGZipBase32768(sourceJson)),
                EmbeddingType.Raw => ("SourceExpander.EmbeddedSourceCode", sourceJson),
                _ => throw new InvalidDataException(),
            };
        }
    }
    public IEnumerable<(string Key, string Value)> EnumerateAssemblyMetadata() => ParseEmbeddingAssemblyMetadata().EnumerateMetadatas();

    private EmbeddingAssemblyMetadata ParseEmbeddingAssemblyMetadata()
    {
        var result = new EmbeddingAssemblyMetadata();

        if (!config.Enabled)
            return result;
        var infos = ResolveFiles();
        cancellationToken.ThrowIfCancellationRequested();

        if (infos.Length is 0)
            return new EmbeddingAssemblyMetadata
            {
                IsEnabled = true,
                EmbeddedData = EmbeddedData.Empty with { AssemblyName = compilation.AssemblyName ?? Guid.NewGuid().ToString() },
            };

        cancellationToken.ThrowIfCancellationRequested();

        result.IsEnabled = true;
        cancellationToken.ThrowIfCancellationRequested();

        var typeNames = infos.SelectMany(s => s.TypeNames).Distinct();
        var namespaceSet = new HashSet<string>();

        foreach (var typeName in typeNames)
        {
            var length = typeName.LastIndexOf('.');
            if (length > 0)
            {
                var maybeNamespaceName = typeName.Substring(0, length);

                // For nested types, `maybeNamespaceName` is not a namespace.
                if (!typeNames.Contains(maybeNamespaceName))
                    namespaceSet.Add(maybeNamespaceName);
            }
            cancellationToken.ThrowIfCancellationRequested();
        }

        var namespaces = namespaceSet.ToArray();
        Array.Sort(namespaces, StringComparer.Ordinal);
        cancellationToken.ThrowIfCancellationRequested();

        return new EmbeddingAssemblyMetadata
        {
            IsEnabled = true,
            EmbeddingType = config.EmbeddingType,
            EmbeddedData = new EmbeddedData(
                EmbeddedNamespaces: ImmutableArray.Create(namespaces),
                CSharpVersion: (config.LanguageVersion ?? parseOptions.LanguageVersion).ToDisplayString(),
                AllowUnsafe: compilation.Options.AllowUnsafe,
                EmbedderVersion: AssemblyUtil.AssemblyVersion,
                Sources: infos,
                AssemblyName: compilation.AssemblyName ?? Guid.NewGuid().ToString()),
        }; ;
    }

    private bool updated = false;
    private void UpdateCompilation()
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (updated) return;
        updated = true;

        var c = compilation;
        foreach (var tree in c.SyntaxTrees.TryParallel(ConcurrentBuild, cancellationToken))
        {
            var semanticModel = compilation.GetSemanticModel(tree, true);
            var newRoot = new EmbedderRewriter(semanticModel, config, reporter, cancellationToken).Visit(tree.GetRoot(cancellationToken))!;
            var newTree = tree.WithRootAndOptions(newRoot, parseOptions);
            c = c.ReplaceSyntaxTree(tree, newTree);
        }
        compilation = c;
    }

    private ImmutableArray<SourceFileInfo> _cacheDependantFiles;
    public ImmutableArray<SourceFileInfo> DependantFiles
    {
        get
        {
            if (_cacheDependantFiles.IsDefault)
            {
                var depSources = ImmutableArray.CreateBuilder<SourceFileInfo>();
                foreach (var (embedded, display, errors) in new AssemblyMetadataResolver(compilation).GetEmbeddedSourceFiles(false, cancellationToken))
                {
                    foreach (var (key, message) in errors)
                    {
                        reporter.ReportDiagnostic(
                            DiagnosticDescriptors.EMBED0006_AnotherAssemblyEmbeddedDataError(display, key, message));
                    }
                    if (embedded.Sources.IsEmpty)
                        continue;
                    if (embedded.EmbedderVersion > AssemblyUtil.AssemblyVersion)
                    {
                        reporter.ReportDiagnostic(
                            DiagnosticDescriptors.EMBED0002_OlderVersion(
                                AssemblyUtil.AssemblyVersion, embedded.AssemblyName, embedded.EmbedderVersion));
                    }
                    depSources.AddRange(embedded.Sources);
                }
                _cacheDependantFiles = depSources.ToImmutable();
                cancellationToken.ThrowIfCancellationRequested();
            }
            return _cacheDependantFiles;
        }
    }
    private ImmutableArray<SourceFileInfoRaw> _cacheResolvedInfoRaws;
    public ImmutableArray<SourceFileInfoRaw> ResolvedInfoRaws
    {
        get
        {
            if (_cacheResolvedInfoRaws.IsDefault)
            {
                UpdateCompilation();
                cancellationToken.ThrowIfCancellationRequested();

                var rawInfos = compilation.SyntaxTrees.TryParallel(ConcurrentBuild, cancellationToken)
                    .Select(ParseSource)
                    .Where(info => info.DefinedTypeNames.Any())
                    .Where(info => config.IsMatch(info.SyntaxTree.FilePath))
                    .ToArray();

                switch (config.EmbeddingFileNameType)
                {
                    case EmbeddingFileNameType.WithoutCommonPrefix:
                        {
                            var commonPrefix = ResolveCommomPrefix(rawInfos.Select(r => r.FileName));
                            var prefix = $"{compilation.AssemblyName}>";
                            for (int i = 0; i < rawInfos.Length; i++)
                            {
                                var newName = string.IsNullOrEmpty(commonPrefix) ? prefix + rawInfos[i].FileName : rawInfos[i].FileName.Replace(commonPrefix, prefix);
                                rawInfos[i] = rawInfos[i].WithFileName(newName);
                            }
                        }
                        break;
                    default:
                        break;
                }
                _cacheResolvedInfoRaws = ImmutableArray.Create(rawInfos);
                cancellationToken.ThrowIfCancellationRequested();
            }
            return _cacheResolvedInfoRaws;
        }
    }

    private ImmutableArray<SourceFileInfo> _cacheResolvedFiles;
    public ImmutableArray<SourceFileInfo> ResolveFiles()
    {
        if (!_cacheResolvedFiles.IsDefault)
            return _cacheResolvedFiles;
        if (!config.Enabled)
            return ImmutableArray<SourceFileInfo>.Empty;

        cancellationToken.ThrowIfCancellationRequested();

        var infos = ResolveRaw(ResolvedInfoRaws, DependantFiles);
        Array.Sort(infos, (info1, info2) => StringComparer.OrdinalIgnoreCase.Compare(info1.FileName, info2.FileName));

        cancellationToken.ThrowIfCancellationRequested();

        var newParseOptions = parseOptions;
        if (config.LanguageVersion is { } langver)
            newParseOptions = newParseOptions.WithLanguageVersion(langver);

        _cacheResolvedFiles = ImmutableArray.Create(infos);
        if (ValidationHelpers.EnumerateEmbeddedSourcesErrors(
            _cacheResolvedFiles, compilation.Options, compilation.References,
            newParseOptions, cancellationToken).ToArray()
            is { Length: > 0 } diagnostics)
        {
            foreach (var d in diagnostics)
            {
                var file = d.Location?.SourceTree?.FilePath;
                if (file is null || d is { WarningLevel: > 0 } or { Id: "CS0759" or "CS8795" or "CS0103" })
                    continue;

                if (d.GetMessage() is string message)
                    reporter.ReportDiagnostic(
                        DiagnosticDescriptors.EMBED0004_ErrorEmbeddedSource(file, message));
            }
        }

        return _cacheResolvedFiles;
    }

    private SourceFileInfoRaw ParseSource(SyntaxTree tree)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var typeFindAndUnusedUsingRemoverResult = new TypeFindAndUnusedUsingRemover()
            .Visit(tree, compilation, cancellationToken);

        var newTree = typeFindAndUnusedUsingRemoverResult.SyntaxTree ?? throw new Exception($"Syntax tree of {tree.FilePath} is invalid");
        cancellationToken.ThrowIfCancellationRequested();

        var minifiedNode = new TriviaFormatter(config.MinifyLevel).Visit(newTree.GetRoot(cancellationToken));
        string minifiedCode = minifiedNode.ToString();

        if (ValidationHelpers.CompareSyntax(
            minifiedNode,
            CSharpSyntaxTree.ParseText(minifiedCode,
            parseOptions,
            cancellationToken: cancellationToken).GetRoot(cancellationToken)) is { } diff)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var diffStr = diff.ToString();

            while (diffStr.Length < 10)
            {
                diff = diff.Parent;
                if (diff is null)
                    break;
                diffStr = diff.ToString();
            }

            reporter.ReportDiagnostic(
                DiagnosticDescriptors.EMBED0005_EmbeddedSourceDiff(diffStr));
        }
        return new SourceFileInfoRaw(tree,
                    tree.FilePath,
                    typeFindAndUnusedUsingRemoverResult.DefinedTypeNames,
                    typeFindAndUnusedUsingRemoverResult.UsedTypes,
                    typeFindAndUnusedUsingRemoverResult.RootUsings,
                    minifiedCode);
    }
    private SourceFileInfo[] ResolveRaw(ImmutableArray<SourceFileInfoRaw> infos, IEnumerable<SourceFileInfo> otherInfos)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var dependencyInfo = new Dictionary<string, HashSet<string>>();
        foreach (var info in infos)
            foreach (var type in info.DefinedTypeNames)
            {
                if (!dependencyInfo.TryGetValue(type, out var deps))
                    dependencyInfo[type] = deps = [];
                deps.Add(info.FileName);
            }
        cancellationToken.ThrowIfCancellationRequested();
        foreach (var info in otherInfos)
            foreach (var type in info.TypeNames)
            {
                if (!dependencyInfo.TryGetValue(type, out var deps))
                    dependencyInfo[type] = deps = [];
                deps.Add(info.FileName);
            }
        var result = new SourceFileInfo[infos.Length];
        for (int i = 0; i < infos.Length; i++)
            result[i] = infos[i].Resolve(dependencyInfo);
        return result;
    }

    public static string ResolveCommomPrefix(IEnumerable<string> strs)
    {
        var min = strs.FirstOrDefault();
        var max = min;

        if (min is null) return "";
        foreach (var s in strs.Skip(1))
        {
            if (StringComparer.Ordinal.Compare(min, s) > 0)
                min = s;
            else if (StringComparer.Ordinal.Compare(max, s) < 0)
                max = s;
        }

        if (min == max)
            return min.Substring(0, min.Length - Path.GetFileName(min).Length);

        for (int i = 0; i < min.Length && i < max.Length; i++)
        {
            if (min[i] != max[i])
                return min.Substring(0, i);
        }
        return min;
    }

}
