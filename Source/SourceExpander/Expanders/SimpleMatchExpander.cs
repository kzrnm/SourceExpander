using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SourceExpander.Expanders.Utils;

namespace SourceExpander.Expanders
{
    internal class SimpleMatchExpander : Expander
    {
        public SimpleMatchExpander(string code, SourceFileContainer sourceFileContainer)
            : base(code, sourceFileContainer) { }

        private SyntaxTree? _origTree;
        protected SyntaxTree OrigTree => _origTree ??= CSharpSyntaxTree.ParseText(OrigCode);

        private ReadOnlyCollection<string>? linesCache;
        public override IEnumerable<string> ExpandedLines()
        {
            IEnumerable<string> Impl()
            {
                var origRoot = (CompilationUnitSyntax)OrigTree.GetRoot();
                var usings = new HashSet<string>(origRoot.Usings.Select(u => u.ToString().Trim()));

                var remover = new UsingRemover();
                var newBody = remover.Visit(origRoot).ToString();

                var requiedFiles = SourceFileContainer.ResolveDependency(GetRequiredSources());
                usings.UnionWith(requiedFiles.SelectMany(s => s.Usings));


                var sortedUsings = ExpanderUtil.SortedUsings(usings);
                foreach (var u in sortedUsings)
                    yield return u;

                using var sr = new StringReader(newBody);
                var line = sr.ReadLine();
                while (line != null)
                {
                    yield return line;
                    line = sr.ReadLine();
                }

                yield return "#region Expanded";
                foreach (var body in requiedFiles.SelectMany(s => ExpanderUtil.ToLines(s.CodeBody)))
                    yield return body;

                yield return "#region NamespaceForUsing";
                foreach (var u in sortedUsings)
                {
                    if (ExpanderUtil.ParseNamespace(u) is { } ns)
                    {
                        yield return $"namespace {ns}{{}}";
                    }
                }
                yield return "#endregion NamespaceForUsing";
                yield return "#endregion Expanded";
            }
            return linesCache ??= Array.AsReadOnly(Impl().ToArray());
        }

        private IEnumerable<SourceFileInfo> GetRequiredSources()
        {
            var simpleNames = OrigTree.GetRoot().DescendantNodes()
                .OfType<SimpleNameSyntax>()
                .Select(s => s.Identifier.ToString())
                .Distinct()
                .ToArray();
            return SourceFileContainer
                .Where(s => s.TypeNames.Select(ExpanderUtil.ToSimpleClassName).Intersect(simpleNames).Any())
                .ToArray();
        }
    }
}
