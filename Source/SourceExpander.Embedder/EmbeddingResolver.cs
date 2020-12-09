using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using SourceExpander.Roslyn;

namespace SourceExpander
{
    public class EmbeddingResolver
    {
        private readonly CSharpCompilation compilation;
        private readonly CSharpParseOptions parseOptions;
        private readonly IDiagnosticReporter reporter;
        private readonly CancellationToken cancellationToken;
        public EmbeddingResolver(
            CSharpCompilation compilation,
            CSharpParseOptions parseOptions,
            IDiagnosticReporter reporter,
            CancellationToken cancellationToken = default)
        {
            var specificDiagnosticOptions = new Dictionary<string, ReportDiagnostic>
            {
                { "CS8019", ReportDiagnostic.Error },
                { "CS0105", ReportDiagnostic.Error },
            };
            var opts = compilation.Options
                .WithSpecificDiagnosticOptions(specificDiagnosticOptions);
            this.compilation = compilation.WithOptions(opts);
            this.parseOptions = parseOptions;
            this.reporter = reporter;
            this.cancellationToken = cancellationToken;
        }
        public IEnumerable<(string name, SourceText sourceText)> EnumerateEmbeddingSources()
        {
            var infos = ResolveFiles();
            if (infos.Length == 0)
                yield break;

            var json = ToJson(infos);
            var gZipBase32768 = SourceFileInfoUtil.ToGZipBase32768(json);
            var embbeddingMetadata = new Dictionary<string, string>
            {
                { "SourceExpander.EmbedderVersion", AssemblyUtil.AssemblyVersion.ToString() },
                { "SourceExpander.EmbeddedSourceCode.GZipBase32768", gZipBase32768 },
                { "SourceExpander.EmbeddedLanguageVersion", parseOptions.LanguageVersion.ToDisplayString()},
            };

            if (compilation.Options.AllowUnsafe)
            {
                embbeddingMetadata["SourceExpander.EmbeddedAllowUnsafe"] = "true";
            }

            yield return (
                "EmbeddedSourceCode.Metadata.Generated.cs",
                SourceText.From(MakeAssemblyMetadataAttributes(embbeddingMetadata), Encoding.UTF8)
            );
        }

        public SourceFileInfo[] ResolveFiles()
        {
            var sources = new List<SourceFileInfo>();
            foreach (var embedded in AssemblyMetadataUtil.GetEmbeddedSourceFiles(compilation))
            {
                if (embedded.EmbedderVersion > AssemblyUtil.AssemblyVersion)
                {
                    reporter.ReportDiagnostic(
                        Diagnostic.Create(DiagnosticDescriptors.EMBED0001, Location.None,
                        AssemblyUtil.AssemblyVersion, embedded.AssemblyName, embedded.EmbedderVersion));
                }
                sources.AddRange(embedded.Sources);
            }

            var commonPrefix = ResolveCommomFileNamePrefix(compilation);
            var infos = ResolveRaw(
                compilation.SyntaxTrees.Select(tree => ParseSource(tree, commonPrefix)),
                sources)
                .Where(info => info.TypeNames.Any())
                .ToArray();
            Array.Sort(infos, (info1, info2) => StringComparer.OrdinalIgnoreCase.Compare(info1.FileName, info2.FileName));
            return infos;
        }

        private IEnumerable<SourceFileInfo> ResolveRaw(IEnumerable<SourceFileInfoRaw> infos, IEnumerable<SourceFileInfo> otherInfos)
        {
            var dependencyInfo = infos.Select(s => new SourceFileInfoSlim(s))
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
        private SourceFileInfoRaw ParseSource(SyntaxTree tree, string commonPrefix)
        {
            var semanticModel = compilation.GetSemanticModel(tree, true);
            var root = (CompilationUnitSyntax)tree.GetRoot(cancellationToken);
            var unusedUsingsSpans = new HashSet<TextSpan>(semanticModel
                .GetDiagnostics(null)
                .Where(d => d.Id == "CS8019" || d.Id == "CS0105" || d.Id == "CS0246")
                .Select(d => d.Location.SourceSpan));
            var usings = root.Usings.Where(u => !unusedUsingsSpans.Contains(u.Span)).Select(u => u.ToString().Trim()).ToArray();

            var remover = new MinifyRewriter();
            var newRoot = (CompilationUnitSyntax)remover.Visit(root)!;

            var prefix = $"{compilation.AssemblyName}>";
            var fileName = string.IsNullOrEmpty(commonPrefix) ?
                prefix + tree.FilePath :
                tree.FilePath.Replace(commonPrefix, prefix);

            var typeNames = root.DescendantNodes()
                .Where(s => s is BaseTypeDeclarationSyntax || s is DelegateDeclarationSyntax)
                .Select(syntax => semanticModel.GetDeclaredSymbol(syntax, cancellationToken)?.ToDisplayString())
                .OfType<string>()
                .Distinct()
                .ToArray();

            var bodyTree = CSharpSyntaxTree.ParseText(newRoot.ToString(), cancellationToken: cancellationToken);

            return new SourceFileInfoRaw(tree, fileName, typeNames, usings,
                remover.Visit(bodyTree.GetRoot(cancellationToken).WithoutTrivia())!.ToString());
        }

        public bool HasType(string typeFullName)
            => compilation.GetTypeByMetadataName(typeFullName) != null;

        public static string ResolveCommomFileNamePrefix(Compilation compilation)
            => ResolveCommomPrefix(compilation.SyntaxTrees.Select(tree => tree.FilePath));
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


        static string ToJson(IEnumerable<SourceFileInfo> infos)
        {
            var serializer = new DataContractJsonSerializer(typeof(IEnumerable<SourceFileInfo>));
            using var ms = new MemoryStream();
            serializer.WriteObject(ms, infos);
            return Encoding.UTF8.GetString(ms.ToArray());
        }
        static string MakeAssemblyMetadataAttributes(IEnumerable<KeyValuePair<string, string>> dic)
        {
            var sb = new StringBuilder("using System.Reflection;");
            foreach (var p in dic)
            {
                sb.Append("[assembly: AssemblyMetadataAttribute(");
                sb.Append(p.Key.ToLiteral());
                sb.Append(",");
                sb.Append(p.Value.ToLiteral());
                sb.AppendLine(")]");
            }
            return sb.ToString();
        }
    }
}
