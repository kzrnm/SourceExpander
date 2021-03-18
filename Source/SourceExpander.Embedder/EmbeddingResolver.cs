using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SourceExpander.Roslyn;

namespace SourceExpander
{
    public class EmbeddingResolver
    {
        private CSharpCompilation compilation;
        private readonly CSharpParseOptions parseOptions;
        private readonly IDiagnosticReporter reporter;
        private readonly EmbedderConfig config;
        private readonly bool ConcurrentBuild;
        private readonly CancellationToken cancellationToken;

        public EmbeddingResolver(EmbeddingContext context)
            : this(context.Compilation, context.ParseOptions, context.Reporter, context.Config, context.CancellationToken)
        { }

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
            public bool EmbeddedAllowUnsafe;
            public Version EmbedderVersion;
            public LanguageVersion EmbeddedLanguageVersion;
            public (string Raw, string GZipBase32768) EmbeddedSourceCode;

            public IEnumerable<(string Key, string Value)> EnumerateMetadatas()
            {
                if (!IsEnabled) yield break;

                if (EmbeddedAllowUnsafe)
                    yield return ("SourceExpander.EmbeddedAllowUnsafe", "true");

                yield return ("SourceExpander.EmbedderVersion", EmbedderVersion.ToString());
                yield return ("SourceExpander.EmbeddedLanguageVersion", EmbeddedLanguageVersion.ToDisplayString());

                yield return EmbeddedSourceCode switch
                {
                    (_, string gz) => ("SourceExpander.EmbeddedSourceCode.GZipBase32768", gz),
                    (string raw, _) => ("SourceExpander.EmbeddedSourceCode", raw),
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
            if (infos.Length == 0)
                return result;
            var json = JsonUtil.ToJson(infos);

            result.IsEnabled = true;
            result.EmbeddedAllowUnsafe = compilation.Options.AllowUnsafe;
            result.EmbedderVersion = AssemblyUtil.AssemblyVersion;
            result.EmbeddedLanguageVersion = parseOptions.LanguageVersion;

            switch (config.EmbeddingType)
            {
                case EmbeddingType.Raw:
                    result.EmbeddedSourceCode.Raw = json;
                    break;
                default:
                    result.EmbeddedSourceCode.GZipBase32768 = SourceFileInfoUtil.ToGZipBase32768(json);
                    break;
            }

            return result;
        }

        private bool updated = false;
        private void VerifyCompilation()
        {
            if (compilation.Options.NullableContextOptions.AnnotationsEnabled())
                reporter.ReportDiagnostic(
                    Diagnostic.Create(DiagnosticDescriptors.EMBED0007_NullableProject, Location.None));
        }
        private void UpdateCompilation()
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (updated) return;
            updated = true;
            SyntaxTree[] newTrees;
            if (ConcurrentBuild)
                newTrees = compilation.SyntaxTrees.AsParallel(cancellationToken)
                    .Select(Rewrited).ToArray();
            else
                newTrees = compilation.SyntaxTrees.Do(_ => cancellationToken.ThrowIfCancellationRequested())
                    .Select(Rewrited).ToArray();
            compilation = compilation.RemoveAllSyntaxTrees().AddSyntaxTrees(newTrees);

            SyntaxTree Rewrited(SyntaxTree tree)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var semanticModel = compilation.GetSemanticModel(tree, true);
                var newRoot = new EmbedderRewriter(semanticModel, config, reporter, cancellationToken).Visit(tree.GetRoot(cancellationToken))!;
                return tree.WithRootAndOptions(newRoot, parseOptions);
            }
        }
        private ImmutableArray<SourceFileInfo> _cacheResolvedFiles;
        public ImmutableArray<SourceFileInfo> ResolveFiles()
        {
            if (!_cacheResolvedFiles.IsDefault)
                return _cacheResolvedFiles;
            if (!config.Enabled)
                return _cacheResolvedFiles = ImmutableArray.Create<SourceFileInfo>();

            cancellationToken.ThrowIfCancellationRequested();
            var depSources = new List<SourceFileInfo>();
            foreach (var (embedded, display, errors) in new AssemblyMetadataResolver(compilation).GetEmbeddedSourceFiles(cancellationToken))
            {
                foreach (var (key, message) in errors)
                {
                    reporter.ReportDiagnostic(
                        Diagnostic.Create(DiagnosticDescriptors.EMBED0006_AnotherAssemblyEmbeddedDataError, Location.None,
                        display, key, message));
                }
                if (embedded.IsEmpty)
                    continue;
                if (embedded.EmbedderVersion > AssemblyUtil.AssemblyVersion)
                {
                    reporter.ReportDiagnostic(
                        Diagnostic.Create(DiagnosticDescriptors.EMBED0002_OlderVersion, Location.None,
                        AssemblyUtil.AssemblyVersion, embedded.AssemblyName, embedded.EmbedderVersion));
                }
                depSources.AddRange(embedded.Sources);
            }
            cancellationToken.ThrowIfCancellationRequested();
            VerifyCompilation();
            UpdateCompilation();
            cancellationToken.ThrowIfCancellationRequested();

            SourceFileInfoRaw[] rawInfos;
            if (ConcurrentBuild)
                rawInfos = compilation.SyntaxTrees.AsParallel(cancellationToken)
                    .Select(ParseSource)
                    .Where(info => info.TypeNames.Any())
                    .ToArray();
            else
                rawInfos = compilation.SyntaxTrees.Do(_ => cancellationToken.ThrowIfCancellationRequested())
                    .Select(ParseSource)
                    .Where(info => info.TypeNames.Any())
                    .ToArray();

            static void WithoutCommonPrefix(SourceFileInfoRaw[] rawInfos, string prefix, string commonPrefix)
            {
                for (int i = 0; i < rawInfos.Length; i++)
                {
                    var newName = string.IsNullOrEmpty(commonPrefix) ? prefix + rawInfos[i].FileName : rawInfos[i].FileName.Replace(commonPrefix, prefix);
                    rawInfos[i] = rawInfos[i].WithFileName(newName);
                }
            }
            WithoutCommonPrefix(rawInfos, $"{compilation.AssemblyName}>", ResolveCommomPrefix(rawInfos.Select(r => r.FileName)));

            var infos = ResolveRaw(rawInfos, depSources).ToArray();
            Array.Sort(infos, (info1, info2) => StringComparer.OrdinalIgnoreCase.Compare(info1.FileName, info2.FileName));

            _cacheResolvedFiles = ImmutableArray.Create(infos);
            if (ValidationHelpers.EnumerateEmbeddedSourcesErrors(
                _cacheResolvedFiles, compilation.Options, compilation.References, parseOptions, cancellationToken).ToArray()
                is { } diagnostics
                && diagnostics.Length > 0)
            {
                foreach (var d in diagnostics)
                {
                    var file = d.Location?.SourceTree?.FilePath;
                    if (file is null || d.WarningLevel > 0)
                        continue;

                    if (d.GetMessage() is string message)
                        reporter.ReportDiagnostic(Diagnostic.Create(
                            DiagnosticDescriptors.EMBED0004_ErrorEmbeddedSource, Location.None,
                            file, message));
                }
            }

            return _cacheResolvedFiles;
        }

        private const string SourceExpander_NotEmbeddingSourceAttributeName = "SourceExpander.NotEmbeddingSourceAttribute";
        private SourceFileInfoRaw ParseSource(SyntaxTree tree)
        {
            var semanticModel = compilation.GetSemanticModel(tree, true);
            var typeFindAndUnusedUsingRemover = new TypeFindAndUnusedUsingRemover(semanticModel, compilation.GetTypeByMetadataName(SourceExpander_NotEmbeddingSourceAttributeName), cancellationToken);

            var newRoot = typeFindAndUnusedUsingRemover.CompilationUnit;
            if (newRoot is null)
                throw new Exception($"Syntax tree of {tree.FilePath} is invalid");

            var usings = typeFindAndUnusedUsingRemover.RootUsings;
            var typeNames = typeFindAndUnusedUsingRemover.DefinedTypeNames;

            SyntaxNode minified = newRoot.NormalizeWhitespace("", " ");
            if (config.EnableMinify)
            {
                minified = TriviaFormatter.Minified(minified)!;
            }
            string minifiedCode = minified!.ToString();

            if (ValidationHelpers.CompareSyntax(newRoot,
                CSharpSyntaxTree.ParseText(minifiedCode,
                parseOptions,
                cancellationToken: cancellationToken).GetRoot(cancellationToken)) is { } diff)
            {
                var diffStr = diff.ToString();

                while (diffStr.Length < 10)
                {
                    diff = diff.Parent;
                    if (diff is null)
                        break;
                    diffStr = diff.ToString();
                }

                reporter.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.EMBED0005_EmbeddedSourceDiff, Location.None, diffStr));
            }
            return new SourceFileInfoRaw(tree, tree.FilePath, typeNames, usings, minifiedCode);
        }
        private IEnumerable<SourceFileInfo> ResolveRaw(SourceFileInfoRaw[] infos, IEnumerable<SourceFileInfo> otherInfos)
        {
            var dependencyInfo = infos.Cast<ISourceFileInfoSlim>()
                .Concat(otherInfos.Select(s => new SourceFileInfoSlim(s)))
                .ToArray();
            IEnumerable<string> GetDependencies(SourceFileInfoRaw raw)
            {
                var tree = raw.SyntaxTree;
                var root = (CompilationUnitSyntax)tree.GetRoot(cancellationToken);

                var semanticModel = compilation.GetSemanticModel(tree, true);
                var typeQueue = new Queue<string>(
                    RoslynUtil.AllTypeNames(semanticModel, tree, cancellationToken));

                var added = new HashSet<string>(raw.TypeNames);
                var dependencies = new HashSet<string>();
                while (typeQueue.Count > 0)
                {
                    var typeName = typeQueue.Dequeue();
                    if (!added.Add(typeName)) continue;

                    dependencies.UnionWith(
                        dependencyInfo
                        .Where(s => s.TypeNames.Contains(typeName))
                        .Select(s => s.FileName));
                }

                return dependencies;
            }


            foreach (var raw in infos)
            {
                yield return new SourceFileInfo
                (
                    raw.FileName,
                    raw.TypeNames,
                    raw.Usings,
                    GetDependencies(raw),
                    raw.CodeBody
                );
            }
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


        private interface ISourceFileInfoSlim
        {
            string FileName { get; }
            ImmutableHashSet<string> TypeNames { get; }
        }
        private class SourceFileInfoRaw : ISourceFileInfoSlim
        {
            public SyntaxTree SyntaxTree { get; }
            public string FileName { get; }
            public ImmutableHashSet<string> TypeNames { get; }
            public IEnumerable<string> Usings { get; }
            public string CodeBody { get; }
            public SourceFileInfoRaw WithFileName(string newName)
                => new(
                    SyntaxTree,
                    newName,
                    TypeNames,
                    Usings,
                    CodeBody);

            public SourceFileInfoRaw(
                SyntaxTree syntaxTree,
                string fileName,
                IEnumerable<string> typeNames,
                IEnumerable<string> usings,
                string codeBody)
            {
                SyntaxTree = syntaxTree;
                FileName = fileName;
                TypeNames = ImmutableHashSet.CreateRange(typeNames);
                Usings = usings;
                CodeBody = codeBody;
            }
        }
        private class SourceFileInfoSlim : ISourceFileInfoSlim
        {
            public string FileName { get; }
            public ImmutableHashSet<string> TypeNames { get; }

            public SourceFileInfoSlim(SourceFileInfo file) : this(file.FileName, file.TypeNames) { }
            public SourceFileInfoSlim(string filename, IEnumerable<string> typeNames)
            {
                FileName = filename;
                TypeNames = ImmutableHashSet.CreateRange(typeNames);
            }
        }
    }
}
