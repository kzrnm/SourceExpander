using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace SourceExpander.Roslyn
{
    internal interface IDiagnosticReporter
    {
        void ReportDiagnostic(Diagnostic diagnostic);
    }
    internal interface ISourceAdder
    {
        void AddSource(string hintName, string source);
        void AddSource(string hintName, SourceText sourceText);
    }
    internal interface IContextWrappter : IDiagnosticReporter, ISourceAdder
    {
        CancellationToken CancellationToken { get; }
    }
}
