using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.CodeAnalysis;
using SourceExpander.Roslyn;

namespace SourceExpander
{
    public class MockDiagnosticReporter : IDiagnosticReporter
    {
        private readonly List<Diagnostic> diagnostics = new();
        public IReadOnlyCollection<Diagnostic> Diagnostics { get; }
        public MockDiagnosticReporter()
        {
            Diagnostics = new ReadOnlyCollection<Diagnostic>(diagnostics);
        }
        public void ReportDiagnostic(Diagnostic diagnostic) => diagnostics.Add(diagnostic);
    }
}
