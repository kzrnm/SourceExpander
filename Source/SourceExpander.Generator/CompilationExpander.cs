using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
            var sb = new StringBuilder();
            var semanticModel = Compilation.GetSemanticModel(origTree, true);
            var origRoot = (CompilationUnitSyntax)origTree.GetRoot(cancellationToken);

            var typeFindAndUnusedUsingRemover = new TypeFindAndUnusedUsingRemover(semanticModel, cancellationToken);
            var newRoot = typeFindAndUnusedUsingRemover.CompilationUnit;
            if (typeFindAndUnusedUsingRemover.UsedTypeNames is not { } typeNames)
                throw new InvalidOperationException($"{nameof(typeNames)} is null");
            var requiedFiles = sourceFileContainer.ResolveDependency(typeNames, false);

            var usings = typeFindAndUnusedUsingRemover.RootUsings
                .Union(requiedFiles.SelectMany(s => s.Usings))
                .ToArray();

            foreach (var u in SourceFileInfoUtil.SortUsings(usings))
                sb.AppendLine(u);

            using var sr = new StringReader(newRoot.ToString());
            var line = sr.ReadLine();
            while (line != null)
            {
                sb.AppendLine(line);
                line = sr.ReadLine();
            }

            sb.AppendLine("#region Expanded by https://github.com/naminodarie/SourceExpander");
            if (!string.IsNullOrEmpty(Config.StaticEmbeddingText))
                sb.AppendLine(Config.StaticEmbeddingText);
            foreach (var s in requiedFiles)
                sb.AppendLine(s.CodeBody);
            sb.AppendLine("#endregion Expanded by https://github.com/naminodarie/SourceExpander");
            return sb.ToString();
        }
    }
}
