using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace SourceExpander.Roslyn
{
    internal class GeneratorExecutionContextWrapper : IContextWrappter
    {
        private readonly GeneratorExecutionContext context;

        public GeneratorExecutionContextWrapper(GeneratorExecutionContext context)
        {
            this.context = context;
        }
        public void ReportDiagnostic(Diagnostic diagnostic) => context.ReportDiagnostic(diagnostic);
        public void AddSource(string hintName, string source) => context.AddSource(hintName, source);
        public void AddSource(string hintName, SourceText sourceText) => context.AddSource(hintName, sourceText);
        public CancellationToken CancellationToken => context.CancellationToken;
    }
}
