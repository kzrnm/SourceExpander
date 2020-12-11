using Microsoft.CodeAnalysis;

namespace SourceExpander.Diagnostics
{
    public static class DiagnosticDescriptors
    {
        internal static readonly DiagnosticDescriptor EMBEDDER0001_UsingStatic = new DiagnosticDescriptor(
            "EMBEDDER0001",
            new LocalizableResourceString(
                nameof(DiagnosticsResources.EMBEDDER0001_Title),
                DiagnosticsResources.ResourceManager,
                typeof(DiagnosticsResources)),
            new LocalizableResourceString(
                nameof(DiagnosticsResources.EMBEDDER0001_Title),
                DiagnosticsResources.ResourceManager,
                typeof(DiagnosticsResources)),
            "Using Directive",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true
            );
        internal static readonly DiagnosticDescriptor EMBEDDER0002_UsingAlias = new DiagnosticDescriptor(
            "EMBEDDER0002",
            new LocalizableResourceString(
                nameof(DiagnosticsResources.EMBEDDER0002_Title),
                DiagnosticsResources.ResourceManager,
                typeof(DiagnosticsResources)),
            new LocalizableResourceString(
                nameof(DiagnosticsResources.EMBEDDER0002_Title),
                DiagnosticsResources.ResourceManager,
                typeof(DiagnosticsResources)),
            "Using Directive",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true
            );
    }
}
