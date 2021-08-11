using Microsoft.CodeAnalysis;

namespace SourceExpander.Roslyn
{
    internal interface IDiagnosticReporter
    {
        void ReportDiagnostic(Diagnostic diagnostic);
    }

    internal class GeneratorExecutionContextDiagnosticReporter : IDiagnosticReporter
    {
        private readonly GeneratorExecutionContext context;
        public GeneratorExecutionContextDiagnosticReporter(GeneratorExecutionContext context)
        {
            this.context = context;
        }
        public void ReportDiagnostic(Diagnostic diagnostic) => context.ReportDiagnostic(diagnostic);
    }
    internal class SourceProductionContextDiagnosticReporter : IDiagnosticReporter
    {
        private readonly SourceProductionContext context;
        public SourceProductionContextDiagnosticReporter(SourceProductionContext context)
        {
            this.context = context;
        }
        public void ReportDiagnostic(Diagnostic diagnostic) => context.ReportDiagnostic(diagnostic);
    }
}
