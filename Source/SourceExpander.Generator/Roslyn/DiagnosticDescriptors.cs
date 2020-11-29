using Microsoft.CodeAnalysis;
#pragma warning disable RS2008
namespace SourceExpander.Roslyn
{
    public static class DiagnosticDescriptors
    {
        public static readonly DiagnosticDescriptor EXPAND0001 = new DiagnosticDescriptor(
            "EXPAND0001",
            "not found embedded source",
            "not found embedded source",
            "ExpandGenerator",
            DiagnosticSeverity.Info,
            true);
        public static readonly DiagnosticDescriptor EXPAND0002 = new DiagnosticDescriptor(
            "EXPAND0002",
            "expander version is older",
            "expander version({0}) is older than embedder of {1}({2})",
            "EmbedderGenerator",
            DiagnosticSeverity.Warning,
            true);
    }
}
