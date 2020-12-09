using System;
using System.Collections.Generic;
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
        public CompilationExpander(CSharpCompilation compilation, SourceFileContainer sourceFileContainer)
        {
            this.sourceFileContainer = sourceFileContainer;
            var specificDiagnosticOptionsBuilder = ImmutableDictionary.CreateBuilder<string, ReportDiagnostic>();
            specificDiagnosticOptionsBuilder.Add("CS8019", ReportDiagnostic.Error);
            specificDiagnosticOptionsBuilder.Add("CS0105", ReportDiagnostic.Error);
            var opts = compilation.Options
                .WithSpecificDiagnosticOptions(specificDiagnosticOptionsBuilder.ToImmutable());
            Compilation = compilation.WithOptions(opts);
        }

        private CSharpCompilation Compilation { get; }

        public string ExpandCode(SyntaxTree origTree, CancellationToken cancellationToken)
        {
            var sb = new StringBuilder();
            var semanticModel = Compilation.GetSemanticModel(origTree);
            var origRoot = (CompilationUnitSyntax)origTree.GetRoot(cancellationToken);
            var requiedFiles = sourceFileContainer.ResolveDependency(
                RoslynUtil.AllTypeNames(semanticModel, origTree, cancellationToken),
                false);

            var newRoot = (CompilationUnitSyntax)(new MatchSyntaxRemover(
                ImmutableHashSet.CreateRange<SyntaxNode?>(semanticModel
                .GetDiagnostics(null, cancellationToken: cancellationToken)
                .Where(d => d.Id == "CS8019" || d.Id == "CS0105" || d.Id == "CS0246")
                .Select(d => origRoot.FindNode(d.Location.SourceSpan))
                .OfType<UsingDirectiveSyntax>()))
                .Visit(origRoot) ?? throw new InvalidOperationException());

            var usings = new HashSet<string>(newRoot.Usings.Select(u => u.ToString().Trim()));

            var newBody = (new UsingRemover().Visit(newRoot) ?? throw new InvalidOperationException()).ToString();

            usings.UnionWith(requiedFiles.SelectMany(s => s.Usings));


            foreach (var u in SortUsings(usings.ToArray()))
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

        private static string[] SortUsings(string[] usings)
        {
            Array.Sort(usings, (a, b) => StringComparer.Ordinal.Compare(a.TrimEnd(';'), b.TrimEnd(';')));
            return usings;
        }
    }
}
