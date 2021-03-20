using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;

namespace SourceExpander.Roslyn
{
    internal static class ValidationHelpers
    {
        private static IEnumerable<Diagnostic> GetCompilationDiagnostic(Compilation compilation, CancellationToken cancellationToken = default)
        {
            using var ms = new MemoryStream();
            return GetCompilationDiagnostic(compilation.Emit(ms, cancellationToken: cancellationToken));
        }
        private static IEnumerable<Diagnostic> GetCompilationDiagnostic(EmitResult result)
        {
            if (result.Success)
                return Array.Empty<Diagnostic>();
            return result.Diagnostics.EnumerateCompilationError();
        }
        private static IEnumerable<Diagnostic> EnumerateCompilationError(this ImmutableArray<Diagnostic> diagnostics)
            => diagnostics.Where(d => d.IsWarningAsError || d.Severity == DiagnosticSeverity.Error);
        public static bool HasCompilationError(this ImmutableArray<Diagnostic> diagnostics)
            => diagnostics.EnumerateCompilationError().Any();
        public static IEnumerable<Diagnostic> EnumerateEmbeddedSourcesErrors(
            ImmutableArray<SourceFileInfo> sources,
            CSharpCompilationOptions compilationOptions,
            IEnumerable<MetadataReference> references,
            CSharpParseOptions parseOptions,
            CancellationToken cancellationToken = default)
        {
            var embeddedCompilation = CSharpCompilation.Create("NewCompilation",
                sources.Select(s => s.ToSyntaxTree(parseOptions, Encoding.UTF8, cancellationToken)),
                references, compilationOptions);
            return GetCompilationDiagnostic(embeddedCompilation, cancellationToken);
        }

        public static SyntaxNode? CompareSyntax(SyntaxNode orig, SyntaxNode target)
        {
            orig = orig.NormalizeWhitespace(" ", " ");
            target = target.NormalizeWhitespace(" ", " ");
            var origStr = orig.ToString();
            var targetStr = target.ToString();
            if (origStr == targetStr)
                return null;

            var length = Math.Min(origStr.Length, targetStr.Length);
            int i;
            for (i = 0; i < length; i++)
                if (origStr[i] != targetStr[i])
                    break;

            return target.FindNode(new TextSpan(i, 1));
        }
    }
}
