using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace SourceExpander
{
    public static class TestUtil
    {
        public static IEnumerable<(T? First, T? Second)> ZipAndFill<T>(this IEnumerable<T> first, IEnumerable<T> second, T defaultValue = default)
        {
            bool m1, m2;
            var e1 = first.GetEnumerator();
            var e2 = second.GetEnumerator();
            while ((m1 = e1.MoveNext()) | (m2 = e2.MoveNext()))
            {
                var v1 = m1 ? e1.Current : defaultValue;
                var v2 = m2 ? e2.Current : defaultValue;
                yield return (v1, v2);
            }
        }

        public static void TestCompile(string code, CancellationToken cancellationToken = default)
        {
            var tree = CSharpSyntaxTree.ParseText(code, cancellationToken: cancellationToken);
            tree.GetDiagnostics(cancellationToken).Should().BeEmpty();
            var dir = Path.GetDirectoryName(typeof(object).Assembly.Location) ?? throw new InvalidOperationException();
            var pathes = Directory.EnumerateFiles(dir, "*.dll");
            var compilation = CSharpCompilation.Create("compilation",
                syntaxTrees: new[] { tree },
                references: pathes.Select(p => MetadataReference.CreateFromFile(p)));
            var semanticModel = compilation.GetSemanticModel(tree);
            var diagnostics = semanticModel.GetDiagnostics(cancellationToken: cancellationToken)
                .Where(d => d.WarningLevel == 0).ToArray();
            diagnostics.Should().BeEmpty();
        }
    }
}
