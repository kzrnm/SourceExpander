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
            ConcurrentBuild = opts.ConcurrentBuild;
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
                newTrees = compilation.SyntaxTrees.AsParallel().Select(Rewrited).ToArray();
            else
                newTrees = compilation.SyntaxTrees.Select(Rewrited).ToArray();
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

            VerifyCompilation();
            UpdateCompilation();
            var depSources = new List<SourceFileInfo>();
            foreach (var (embedded, display, errors) in new AssemblyMetadataResolver(compilation).GetEmbeddedSourceFiles())
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

            var rawInfos = compilation.SyntaxTrees
                .Select(tree => ParseSource(tree))
                .Where(info => info.TypeNames.Any())
                .ToArray();
            var commonPrefix = ResolveCommomPrefix(rawInfos.Select(r => r.FileName));
            var infos = ResolveRaw(rawInfos, commonPrefix, depSources).ToArray();
            Array.Sort(infos, (info1, info2) => StringComparer.OrdinalIgnoreCase.Compare(info1.FileName, info2.FileName));

            _cacheResolvedFiles = ImmutableArray.Create(infos);
            if (ValidationHelpers.EnumerateEmbeddedSourcesErrors(
                compilation, parseOptions, _cacheResolvedFiles, cancellationToken).ToArray()
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

        private SourceFileInfoRaw ParseSource(SyntaxTree tree)
        {
            var semanticModel = compilation.GetSemanticModel(tree, true);
            var root = (CompilationUnitSyntax)tree.GetRoot(cancellationToken);

            var typeFindAndUnusedUsingRemover = new TypeFindAndUnusedUsingRemover(semanticModel, cancellationToken);

            var newRoot = typeFindAndUnusedUsingRemover.Visit(root);
            if (newRoot is null)
                throw new Exception($"Syntax tree of {tree.FilePath} is invalid");

            var usings = typeFindAndUnusedUsingRemover.RootUsings();

            var typeNames = typeFindAndUnusedUsingRemover.DefinedTypeNames();

            var minified = newRoot.NormalizeWhitespace("", " ");
            if (config.EnableMinify)
            {
                minified = new TriviaFormatter().Visit(minified)!;
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
        private IEnumerable<SourceFileInfo> ResolveRaw(IEnumerable<SourceFileInfoRaw> infos, string commonPrefix, IEnumerable<SourceFileInfo> otherInfos)
        {
            var prefix = $"{compilation.AssemblyName}>";
            string NewName(SourceFileInfoRaw raw)
                => string.IsNullOrEmpty(commonPrefix) ?
                prefix + raw.FileName :
                raw.FileName.Replace(commonPrefix, prefix);
            infos = infos.Select(raw => raw.WithFileName(NewName(raw)));

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

        public bool HasType(string typeFullName)
            => compilation.GetTypeByMetadataName(typeFullName) != null;

        public static string ResolveCommomPrefix(IEnumerable<string> strs)
        {
            var sorted = new SortedSet<string>(strs, StringComparer.Ordinal);
            if (sorted.Count < 1) return "";
            if (sorted.Count < 2)
            {
                var p = sorted.Min;
                var name = Path.GetFileName(p);
                if (!p.EndsWith(name))
                    return "";

                return p.Substring(0, p.Length - name.Length);
            }

            var min = sorted.Min;
            var max = sorted.Max;

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
            public static readonly SourceFileInfoRaw Dummy
                = new(CSharpSyntaxTree.ParseText(""), "Dummy", Array.Empty<string>(), Array.Empty<string>(), "");
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
