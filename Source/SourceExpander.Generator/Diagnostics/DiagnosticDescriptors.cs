using Microsoft.CodeAnalysis;
#pragma warning disable RS2008
namespace SourceExpander
{
    public static class DiagnosticDescriptors
    {
        public static readonly DiagnosticDescriptor EXPAND0001_UnknownError = new(
            "EXPAND0001",
            "Unknown error",
            "Unknown error: {0}",
            "ExpandGenerator",
            DiagnosticSeverity.Warning,
            true);
        public static readonly DiagnosticDescriptor EXPAND0002_ExpanderVersion = new(
            "EXPAND0002",
            "Expander version is older",
            "Expander version({0}) is older than embedder of {1}({2})",
            "ExpandGenerator",
            DiagnosticSeverity.Warning,
            true);
        public static readonly DiagnosticDescriptor EXPAND0003_NotFoundEmbedded = new(
            "EXPAND0003",
            "Not found embedded source",
            "Not found embedded source",
            "ExpandGenerator",
            DiagnosticSeverity.Info,
            true);
        public static readonly DiagnosticDescriptor EXPAND0004_MustBeNewerThanCSharp3 = new(
            "EXPAND0004",
            "C# version must be newer than C# 3",
            "C# version must be newer than C# 3. Compilation's C# version is {0}.",
            "ExpandGenerator",
            DiagnosticSeverity.Info,
            true);
        public static readonly DiagnosticDescriptor EXPAND0005_NewerCSharpVersion = new(
            "EXPAND0005",
            "C# version is older than embedded",
            "C# version({0}) is older than embedded {1}({2})",
            "ExpandGenerator",
            DiagnosticSeverity.Warning,
            true);
        public static readonly DiagnosticDescriptor EXPAND0006_AllowUnsafe = new(
            "EXPAND0006",
            "maybe needs AllowUnsafeBlocks",
            "maybe needs AllowUnsafeBlocks because {0} has AllowUnsafeBlocks",
            "ExpandGenerator",
            DiagnosticSeverity.Warning,
            true);
        public static readonly DiagnosticDescriptor EXPAND0007_ParseConfigError = new(
            "EXPAND0007",
            "Error config file",
            "Error config file: {0}",
            "ExpandGenerator",
            DiagnosticSeverity.Error,
            true);
        public static readonly DiagnosticDescriptor EXPAND0008_EmbeddedDataError = new(
            "EXPAND0008",
            "Invalid embedded data",
            "Invalid embedded data: Key: {0}, Message: {1}",
            "ExpandGenerator",
            DiagnosticSeverity.Warning,
            true);
    }
}
