using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SourceExpander.Roslyn;

namespace SourceExpander
{
    internal class CompilationExpander
    {
        private readonly SourceFileContainer sourceFileContainer;
        public CompilationExpander(CSharpCompilation compilation, SourceFileContainer sourceFileContainer)
        {
            this.sourceFileContainer = sourceFileContainer;
            var specificDiagnosticOptions = new Dictionary<string, ReportDiagnostic>
            {
                { "CS8019", ReportDiagnostic.Error },
                { "CS0105", ReportDiagnostic.Error },
            };
            var opts = compilation.Options
                .WithSpecificDiagnosticOptions(specificDiagnosticOptions);
            Compilation = compilation.WithOptions(opts);
        }

        private CSharpCompilation Compilation { get; }

        public string ExpandCode(SyntaxTree origTree)
        {
            var sb = new StringBuilder();
            var semanticModel = Compilation.GetSemanticModel(origTree);
            var origRoot = origTree.GetRoot();
            var requiedFiles = sourceFileContainer.ResolveDependency(
                origRoot.DescendantNodes()
                .Select(s => GetTypeNameFromSymbol(semanticModel.GetSymbolInfo(s).Symbol))
                .OfType<string>(),
                false);

            var newRoot = (CompilationUnitSyntax)(new MatchSyntaxRemover(
                semanticModel
                .GetDiagnostics(null)
                .Where(d => d.Id == "CS8019" || d.Id == "CS0105" || d.Id == "CS0246")
                .Select(d => origRoot.FindNode(d.Location.SourceSpan))
                .OfType<UsingDirectiveSyntax>())
                .Visit(origRoot) ?? throw new InvalidOperationException());

            var usings = new HashSet<string>(newRoot.Usings.Select(u => u.ToString().Trim()));

            var remover = new UsingRemover();
            var newBody = remover.Visit(newRoot).ToString();

            usings.UnionWith(requiedFiles.SelectMany(s => s.Usings));


            var sortedUsings = SortedUsings(usings);
            foreach (var u in sortedUsings)
                sb.AppendLine(u);

            using var sr = new StringReader(newBody);
            var line = sr.ReadLine();
            while (line != null)
            {
                sb.AppendLine(line);
                line = sr.ReadLine();
            }

            sb.AppendLine("#region Expanded");
            foreach (var s in requiedFiles)
                sb.AppendLine(s.CodeBody);
            sb.AppendLine("#endregion Expanded");
            return sb.ToString();
        }

        public static string[] SortedUsings(IEnumerable<string> usings)
        {
            var arr = usings.ToArray();
            Array.Sort(arr, (a, b) => StringComparer.Ordinal.Compare(a.TrimEnd(';'), b.TrimEnd(';')));
            return arr;
        }

        private static string? GetTypeNameFromSymbol(ISymbol? symbol)
        {
            if (symbol == null) return null;
            if (symbol is INamedTypeSymbol named)
            {
                return named.ConstructedFrom.ToDisplayString();
            }
            return symbol.ContainingType?.ConstructedFrom?.ToDisplayString() ?? symbol.ToDisplayString();
        }
    }
}
