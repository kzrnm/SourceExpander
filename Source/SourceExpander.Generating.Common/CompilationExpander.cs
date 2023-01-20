using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SourceExpander.Roslyn;

namespace SourceExpander
{
    internal class CompilationExpander
    {
        private readonly SourceFileContainer sourceFileContainer;
        private ExpandConfig Config { get; }
        public CompilationExpander(CSharpCompilation compilation, SourceFileContainer sourceFileContainer, ExpandConfig config)
        {
            this.sourceFileContainer = sourceFileContainer;
            var specificDiagnosticOptionsBuilder = ImmutableDictionary.CreateBuilder<string, ReportDiagnostic>();
            specificDiagnosticOptionsBuilder.Add("CS8019", ReportDiagnostic.Error);
            specificDiagnosticOptionsBuilder.Add("CS0105", ReportDiagnostic.Error);
            var opts = compilation.Options
                .WithSpecificDiagnosticOptions(specificDiagnosticOptionsBuilder.ToImmutable());
            Compilation = compilation.WithOptions(opts);
            Config = config;
        }

        private CSharpCompilation Compilation { get; }

        public string ExpandCode(SyntaxTree origTree, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var semanticModel = Compilation.GetSemanticModel(origTree, true);
            var origRoot = origTree.GetCompilationUnitRoot(cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();
            var typeFindAndUnusedUsingRemover = new TypeFindAndUnusedUsingRemover(semanticModel, cancellationToken);
            var newRoot = typeFindAndUnusedUsingRemover.CompilationUnit;
            if (typeFindAndUnusedUsingRemover.UsedTypes is null)
                throw new InvalidOperationException($"{nameof(typeFindAndUnusedUsingRemover.UsedTypes)} is null");

            cancellationToken.ThrowIfCancellationRequested();
            var requiedFiles = sourceFileContainer.ResolveDependency(typeFindAndUnusedUsingRemover.UsedTypes, cancellationToken).ToArray();
            Array.Sort(requiedFiles, (f1, f2) => StringComparer.OrdinalIgnoreCase.Compare(f1.FileName, f2.FileName));

            cancellationToken.ThrowIfCancellationRequested();
            var usings = typeFindAndUnusedUsingRemover.RootUsings
                .Union(requiedFiles.SelectMany(s => s.Usings))
                .ToArray();
            cancellationToken.ThrowIfCancellationRequested();

            var importButUnusedNamespaces = ImmutableArray.CreateBuilder<string>();
            {
                static IEnumerable<string> TypeNameToNamespaces(string typeName)
                {
                    var array = typeName.Split('.');
                    var sb = new StringBuilder(typeName.Length);
                    for (int i = 0; i + 1 < array.Length; i++)
                    {
                        sb.Append(array[i]);
                        yield return sb.ToString();
                        sb.Append('.');
                    }
                }
                var usedNamespaces = requiedFiles.SelectMany(r => r.TypeNames).SelectMany(TypeNameToNamespaces).ToImmutableHashSet();
                cancellationToken.ThrowIfCancellationRequested();

                foreach (var u in usings)
                {
                    if (u.Length >= 7)
                    {
                        var ns = u.Substring(6, u.Length - 7); // namespace
                        if (sourceFileContainer.DefinedNamespaces.Contains(ns)
                            && !usedNamespaces.Contains(ns))
                            importButUnusedNamespaces.Add(ns);
                    }
                    cancellationToken.ThrowIfCancellationRequested();
                }
            }
            cancellationToken.ThrowIfCancellationRequested();

            var sb = new StringBuilder();
            foreach (var u in SourceFileInfoUtil.SortUsings(usings))
                sb.AppendLine(u);

            cancellationToken.ThrowIfCancellationRequested();


            using var sr = new StringReader(newRoot.ToString());
            var line = sr.ReadLine();
            while (line != null)
            {
                sb.AppendLine(line);
                line = sr.ReadLine();
            }

            sb.AppendLine("#region Expanded by https://github.com/kzrnm/SourceExpander");
            if (!string.IsNullOrEmpty(Config.StaticEmbeddingText))
                sb.AppendLine(Config.StaticEmbeddingText);

            if (Config.ExpandingByGroup)
            {
                var groupedCodes = new Dictionary<string, List<string>>();
                foreach (var s in requiedFiles)
                {
                    var match = Regex.Match(s.FileName, "^[^>]+");
                    var assemblyName = match?.Value ?? "<unknown assembly>";
                    if (!groupedCodes.TryGetValue(assemblyName, out var list))
                    {
                        list = groupedCodes[assemblyName] = new List<string>();
                    }
                    list.Add(s.CodeBody);
                }
                foreach (var g in groupedCodes)
                {
                    var assemblyName = g.Key;
                    sb.Append("#region Assembly:").AppendLine(assemblyName);
                    foreach (var s in g.Value)
                        sb.AppendLine(s);
                    sb.Append("#endregion Assembly:").AppendLine(assemblyName);
                }
            }
            else
            {
                foreach (var s in requiedFiles)
                    sb.AppendLine(s.CodeBody);
            }
            cancellationToken.ThrowIfCancellationRequested();

            foreach (var ns in importButUnusedNamespaces)
            {
                sb.Append("namespace ").Append(ns).AppendLine("{}");
            }
            sb.AppendLine("#endregion Expanded by https://github.com/kzrnm/SourceExpander");
            return sb.ToString();
        }

        public SourceFileInfo[] ResolveDependency(SyntaxTree origTree, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var semanticModel = Compilation.GetSemanticModel(origTree, true);
            var origRoot = origTree.GetCompilationUnitRoot(cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();
            var typeFindAndUnusedUsingRemover = new TypeFindAndUnusedUsingRemover(semanticModel, cancellationToken);
            var newRoot = typeFindAndUnusedUsingRemover.CompilationUnit;
            if (typeFindAndUnusedUsingRemover.UsedTypes is null)
                throw new InvalidOperationException($"{nameof(typeFindAndUnusedUsingRemover.UsedTypes)} is null");

            cancellationToken.ThrowIfCancellationRequested();
            var requiedFiles = sourceFileContainer.ResolveDependency(typeFindAndUnusedUsingRemover.UsedTypes, cancellationToken).ToArray();
            Array.Sort(requiedFiles, (f1, f2) => StringComparer.OrdinalIgnoreCase.Compare(f1.FileName, f2.FileName));

            return requiedFiles;
        }
    }
}
