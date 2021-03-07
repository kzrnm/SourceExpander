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

            this.parseOptions = parseOptions.WithDocumentationMode(DocumentationMode.Diagnose);
            this.compilation = compilation
                .AddSyntaxTrees(
                    CompileTimeTypeMaker.CreateSyntaxes(this.parseOptions))
                .WithOptions(opts);
            this.reporter = reporter;
            this.config = config;
            this.cancellationToken = cancellationToken;
        }
        public ImmutableDictionary<string, string> EnumerateAssemblyMetadata()
        {
            if (!config.Enabled)
                return ImmutableDictionary<string, string>.Empty;
            var infos = ResolveFiles();
            if (infos.Length == 0)
                return ImmutableDictionary<string, string>.Empty;

            var embbeddingMetadataBuilder = ImmutableDictionary.CreateBuilder<string, string>();
            embbeddingMetadataBuilder.Add("SourceExpander.EmbedderVersion", AssemblyUtil.AssemblyVersion.ToString());
            embbeddingMetadataBuilder.Add("SourceExpander.EmbeddedLanguageVersion", parseOptions.LanguageVersion.ToDisplayString());

            var json = JsonUtil.ToJson(infos);
            switch (config.EmbeddingType)
            {
                case EmbeddingType.Raw:
                    embbeddingMetadataBuilder.Add("SourceExpander.EmbeddedSourceCode", json);
                    break;
                default:
                    embbeddingMetadataBuilder.Add("SourceExpander.EmbeddedSourceCode.GZipBase32768", SourceFileInfoUtil.ToGZipBase32768(json));
                    break;
            }

            if (compilation.Options.AllowUnsafe)
            {
                embbeddingMetadataBuilder.Add("SourceExpander.EmbeddedAllowUnsafe", "true");
            }
            return embbeddingMetadataBuilder.ToImmutable();
        }

        private bool updated = false;
        private void UpdateCompilation()
        {
            if (updated)
                return;
            updated = true;
            var newCompilation = compilation;
            var trees = compilation.SyntaxTrees;
            foreach (var tree in trees)
            {
                var semanticModel = compilation.GetSemanticModel(tree, true);
                var newRoot = new EmbedderRewriter(semanticModel, config, cancellationToken).Visit(tree.GetRoot(cancellationToken));
                newCompilation = newCompilation.ReplaceSyntaxTree(tree,
                    tree.WithRootAndOptions(newRoot, parseOptions));
            }
            compilation = newCompilation;
            return;
        }
        private ImmutableArray<SourceFileInfo> _cacheResolvedFiles;
        public ImmutableArray<SourceFileInfo> ResolveFiles()
        {
            if (!_cacheResolvedFiles.IsDefault)
                return _cacheResolvedFiles;
            if (!config.Enabled || compilation.GetDiagnostics(cancellationToken).HasCompilationError())
                return _cacheResolvedFiles = ImmutableArray.Create<SourceFileInfo>();
            UpdateCompilation();
            var sources = new List<SourceFileInfo>();
            foreach (var embedded in AssemblyMetadataUtil.GetEmbeddedSourceFiles(compilation))
            {
                if (embedded.EmbedderVersion > AssemblyUtil.AssemblyVersion)
                {
                    reporter.ReportDiagnostic(
                        Diagnostic.Create(DiagnosticDescriptors.EMBED0001_OlderVersion, Location.None,
                        AssemblyUtil.AssemblyVersion, embedded.AssemblyName, embedded.EmbedderVersion));
                }
                sources.AddRange(embedded.Sources);
            }

            var commonPrefix = ResolveCommomFileNamePrefix(compilation);
            var infos = ResolveRaw(
                compilation.SyntaxTrees
                .Where(tree => !tree.IsCompileTimeType())
                .Select(tree => ParseSource(tree, commonPrefix)),
                sources)
                .Where(info => info.TypeNames.Any())
                .ToArray();
            Array.Sort(infos, (info1, info2) => StringComparer.OrdinalIgnoreCase.Compare(info1.FileName, info2.FileName));

            _cacheResolvedFiles = ImmutableArray.Create(infos);
            if (ValidationHelpers.EnumerateEmbeddedSourcesErrors(
                compilation, parseOptions, _cacheResolvedFiles, cancellationToken).ToArray()
                is { } diagnostics
                && diagnostics.Length > 0)
            {
                var messageDic = new Dictionary<string, List<string>>();
                foreach (var d in diagnostics)
                {
                    var file = d.Location?.SourceTree?.FilePath;
                    if (file is null)
                        continue;
                    var message = d.GetMessage();
                    if (!messageDic.TryGetValue(file, out var messages))
                    {
                        messages = messageDic[file] = new List<string>();
                    }
                    messages.Add(message);
                }
                reporter.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.EMBED0004_ErrorEmbeddedSource, Location.None,
                    string.Join(", ", messageDic.Select(p => $"{p.Key}: {{{string.Join(", ", p.Value)}}}"))));
            }

            return _cacheResolvedFiles;
        }

        private SourceFileInfoRaw ParseSource(SyntaxTree tree, string commonPrefix)
        {
            var semanticModel = compilation.GetSemanticModel(tree, true);
            var root = (CompilationUnitSyntax)tree.GetRoot(cancellationToken);

            var typeFindAndUnusedUsingRemover = new TypeFindAndUnusedUsingRemover(semanticModel, cancellationToken);

            var newRoot = typeFindAndUnusedUsingRemover.Visit(root);
            newRoot = new TriviaRemover().Visit(newRoot);
            if (newRoot is null)
                throw new Exception($"Syntax tree of {tree.FilePath} is invalid");

            var usings = typeFindAndUnusedUsingRemover.RootUsings();

            var prefix = $"{compilation.AssemblyName}>";
            var fileName = string.IsNullOrEmpty(commonPrefix) ?
                prefix + tree.FilePath :
                tree.FilePath.Replace(commonPrefix, prefix);

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
            return new SourceFileInfoRaw(tree, fileName, typeNames, usings, minifiedCode);
        }
        private IEnumerable<SourceFileInfo> ResolveRaw(IEnumerable<SourceFileInfoRaw> infos, IEnumerable<SourceFileInfo> otherInfos)
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
                yield return new SourceFileInfo
                (
                    raw.FileName,
                    raw.TypeNames,
                    raw.Usings,
                    GetDependencies(raw),
                    raw.CodeBody
                );
        }

        public bool HasType(string typeFullName)
            => compilation.GetTypeByMetadataName(typeFullName) != null;

        public static string ResolveCommomFileNamePrefix(Compilation compilation)
            => ResolveCommomPrefix(compilation.SyntaxTrees
                .Where(tree => !tree.IsCompileTimeType())
                .Select(tree => tree.FilePath));
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
