using Microsoft.CodeAnalysis;

namespace SourceExpander.Diagnostics
{
    public static class DiagnosticDescriptors
    {
        internal static readonly DiagnosticDescriptor EMBEDDER0001_UsingStatic = new(
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
        internal static readonly DiagnosticDescriptor EMBEDDER0002_UsingAlias = new(
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
        internal static readonly DiagnosticDescriptor EMBEDDER0003_NullableProject = new(
            "EMBEDDER0003",
            new LocalizableResourceString(
                nameof(DiagnosticsResources.EMBEDDER0003_Title),
                DiagnosticsResources.ResourceManager,
                typeof(DiagnosticsResources)),
            new LocalizableResourceString(
                nameof(DiagnosticsResources.EMBEDDER0003_Title),
                DiagnosticsResources.ResourceManager,
                typeof(DiagnosticsResources)),
            "Compilation",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true
            );
        internal static readonly DiagnosticDescriptor EMBEDDER0004_NullableDirective = new(
            "EMBEDDER0004",
            new LocalizableResourceString(
                nameof(DiagnosticsResources.EMBEDDER0004_Title),
                DiagnosticsResources.ResourceManager,
                typeof(DiagnosticsResources)),
            new LocalizableResourceString(
                nameof(DiagnosticsResources.EMBEDDER0004_Title),
                DiagnosticsResources.ResourceManager,
                typeof(DiagnosticsResources)),
            "Compilation",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true
            );

        internal static readonly DiagnosticDescriptor EMBEDDER0005_ExpandEmbedded = new(
            "EMBEDDER0005",
            new LocalizableResourceString(
                nameof(DiagnosticsResources.EMBEDDER0005_Title),
                DiagnosticsResources.ResourceManager,
                typeof(DiagnosticsResources)),
            new LocalizableResourceString(
                nameof(DiagnosticsResources.EMBEDDER0005_Title),
                DiagnosticsResources.ResourceManager,
                typeof(DiagnosticsResources)),
            "Compilation",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true
            );
    }
}
