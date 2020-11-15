using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using SourceExpander.Expanders.Utils;

namespace SourceExpander.Expanders
{
    internal class AllExpander : Expander
    {
        public string OrigCode { get; }
        public AllExpander(string code, SourceFileContainer sourceFileContainer)
            : base(sourceFileContainer)
        {
            OrigCode = code;
        }

        private ReadOnlyCollection<string>? linesCache;
        public override IEnumerable<string> ExpandedLines()
        {
            IEnumerable<string> Impl()
            {
                var usings = new HashSet<string>(SourceFileContainer.SelectMany(s => s.Usings));
                using var sr = new StringReader(OrigCode);

                var line = sr.ReadLine();
                while (line != null)
                {
                    if (string.IsNullOrWhiteSpace(line)) { }
                    else if (line.StartsWith("using"))
                    {
                        usings.Add(line);
                    }
                    else break;
                    line = sr.ReadLine();
                }

                var sortedUsings = ExpanderUtil.SortedUsings(usings);
                foreach (var u in sortedUsings)
                    yield return u;

                while (line != null)
                {
                    yield return line;
                    line = sr.ReadLine();
                }

                yield return "#region Expanded";
                foreach (var body in SourceFileContainer.SelectMany(s => ExpanderUtil.ToLines(s.CodeBody)))
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
    }
}
