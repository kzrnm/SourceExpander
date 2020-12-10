using Microsoft.CodeAnalysis;
#pragma warning disable RS2008
namespace SourceExpander.Roslyn
{
    public static class DiagnosticDescriptors
    {
        public static readonly DiagnosticDescriptor EXPAND0001_NotFoundEmbedded = new DiagnosticDescriptor(
            "EXPAND0001",
            "Not found embedded source",
            "Not found embedded source",
            "ExpandGenerator",
            DiagnosticSeverity.Info,
            true);
        public static readonly DiagnosticDescriptor EXPAND0002_ExpanderVersion = new DiagnosticDescriptor(
            "EXPAND0002",
            "Expander version is older",
            "Expander version({0}) is older than embedder of {1}({2})",
            "EmbedderGenerator",
            DiagnosticSeverity.Warning,
            true);
        public static readonly DiagnosticDescriptor EXPAND0003_MustBeCSharp = new DiagnosticDescriptor(
            "EXPAND0003",
            "Compilation must be C#",
            "Compilation must be C#. Compilation is {0}.",
            "EmbedderGenerator",
            DiagnosticSeverity.Error,
            true);
        public static readonly DiagnosticDescriptor EXPAND0004_MustBeNewerThanCSharp3 = new DiagnosticDescriptor(
            "EXPAND0004",
            "C# version must be newer than C# 3",
            "C# version must be newer than C# 3. Compilation's C# version is {0}.",
            "EmbedderGenerator",
            DiagnosticSeverity.Error,
            true);
        public static readonly DiagnosticDescriptor EXPAND0005_NewerCSharpVersion = new DiagnosticDescriptor(
            "EXPAND0005",
            "C# version is older than embedded",
            "C# version({0}) is older than embedded {1}({2})",
            "EmbedderGenerator",
            DiagnosticSeverity.Warning,
            true);
        public static readonly DiagnosticDescriptor EXPAND0006_AllowUnsafe = new DiagnosticDescriptor(
            "EXPAND0006",
            "maybe needs AllowUnsafeBlocks",
            "maybe needs AllowUnsafeBlocks because {0} has AllowUnsafeBlocks",
            "EmbedderGenerator",
            DiagnosticSeverity.Warning,
            true);
        public static readonly DiagnosticDescriptor EXPAND0007_ParseConfigError = new DiagnosticDescriptor(
            "EXPAND0007",
            "Error config file",
            "Error config file: {0}",
            "EmbedderGenerator",
            DiagnosticSeverity.Error,
            true);
    }
}
