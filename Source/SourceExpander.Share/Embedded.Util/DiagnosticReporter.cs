using Microsoft.CodeAnalysis;

namespace SourceExpander.Roslyn
{
    public interface IDiagnosticReporter
    {
        void ReportDiagnostic(Diagnostic diagnostic);
    }

    public class DiagnosticReporter : IDiagnosticReporter
    {
        private readonly GeneratorExecutionContext context;
        public DiagnosticReporter(GeneratorExecutionContext context)
        {
            this.context = context;
        }
        public void ReportDiagnostic(Diagnostic diagnostic) => context.ReportDiagnostic(diagnostic);
    }
}
