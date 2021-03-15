using Microsoft.CodeAnalysis;
using SourceExpander.Diagnostics;

namespace SourceExpander
{
    public static class DiagnosticDescriptors
    {
        public static readonly DiagnosticDescriptor EXPAND0001_UnknownError = new(
            "EXPAND0001",
            new LocalizableResourceString(
                nameof(DiagnosticsResources.EXPAND0001_Title),
                DiagnosticsResources.ResourceManager,
                typeof(DiagnosticsResources)),
            new LocalizableResourceString(
                nameof(DiagnosticsResources.EXPAND0001_Body),
                DiagnosticsResources.ResourceManager,
                typeof(DiagnosticsResources)),
            "Error",
            DiagnosticSeverity.Warning,
            true);
        public static readonly DiagnosticDescriptor EXPAND0002_ExpanderVersion = new(
            "EXPAND0002",
            new LocalizableResourceString(
                nameof(DiagnosticsResources.EXPAND0002_Title),
                DiagnosticsResources.ResourceManager,
                typeof(DiagnosticsResources)),
            new LocalizableResourceString(
                nameof(DiagnosticsResources.EXPAND0002_Body),
                DiagnosticsResources.ResourceManager,
                typeof(DiagnosticsResources)),
            "Error",
            DiagnosticSeverity.Warning,
            true);
        public static readonly DiagnosticDescriptor EXPAND0003_NotFoundEmbedded = new(
            "EXPAND0003",
            new LocalizableResourceString(
                nameof(DiagnosticsResources.EXPAND0003_Title),
                DiagnosticsResources.ResourceManager,
                typeof(DiagnosticsResources)),
            new LocalizableResourceString(
                nameof(DiagnosticsResources.EXPAND0003_Title),
                DiagnosticsResources.ResourceManager,
                typeof(DiagnosticsResources)),
            "Error",
            DiagnosticSeverity.Info,
            true);
        public static readonly DiagnosticDescriptor EXPAND0004_MustBeNewerThanCSharp3 = new(
            "EXPAND0004",
            new LocalizableResourceString(
                nameof(DiagnosticsResources.EXPAND0004_Title),
                DiagnosticsResources.ResourceManager,
                typeof(DiagnosticsResources)),
            new LocalizableResourceString(
                nameof(DiagnosticsResources.EXPAND0004_Title),
                DiagnosticsResources.ResourceManager,
                typeof(DiagnosticsResources)),
            "Error",
            DiagnosticSeverity.Info,
            true);
        public static readonly DiagnosticDescriptor EXPAND0005_NewerCSharpVersion = new(
            "EXPAND0005",
            new LocalizableResourceString(
                nameof(DiagnosticsResources.EXPAND0005_Title),
                DiagnosticsResources.ResourceManager,
                typeof(DiagnosticsResources)),
            new LocalizableResourceString(
                nameof(DiagnosticsResources.EXPAND0005_Body),
                DiagnosticsResources.ResourceManager,
                typeof(DiagnosticsResources)),
            "Error",
            DiagnosticSeverity.Warning,
            true);
        public static readonly DiagnosticDescriptor EXPAND0006_AllowUnsafe = new(
            "EXPAND0006",
            new LocalizableResourceString(
                nameof(DiagnosticsResources.EXPAND0006_Title),
                DiagnosticsResources.ResourceManager,
                typeof(DiagnosticsResources)),
            new LocalizableResourceString(
                nameof(DiagnosticsResources.EXPAND0006_Body),
                DiagnosticsResources.ResourceManager,
                typeof(DiagnosticsResources)),
            "Error",
            DiagnosticSeverity.Warning,
            true);
        public static readonly DiagnosticDescriptor EXPAND0007_ParseConfigError = new(
            "EXPAND0007",
            new LocalizableResourceString(
                nameof(DiagnosticsResources.EXPAND0007_Title),
                DiagnosticsResources.ResourceManager,
                typeof(DiagnosticsResources)),
            new LocalizableResourceString(
                nameof(DiagnosticsResources.EXPAND0007_Body),
                DiagnosticsResources.ResourceManager,
                typeof(DiagnosticsResources)),
            "Error",
            DiagnosticSeverity.Error,
            true);
        public static readonly DiagnosticDescriptor EXPAND0008_EmbeddedDataError = new(
            "EXPAND0008",
            new LocalizableResourceString(
                nameof(DiagnosticsResources.EXPAND0008_Title),
                DiagnosticsResources.ResourceManager,
                typeof(DiagnosticsResources)),
            new LocalizableResourceString(
                nameof(DiagnosticsResources.EXPAND0008_Body),
                DiagnosticsResources.ResourceManager,
                typeof(DiagnosticsResources)),
            "Error",
            DiagnosticSeverity.Warning,
            true);
    }
}
