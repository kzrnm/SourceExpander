using Microsoft.CodeAnalysis;
#pragma warning disable RS2008
namespace SourceExpander.Roslyn
{
    public static class DiagnosticDescriptors
    {
        public static readonly DiagnosticDescriptor EMBED0001_UnknownError = new(
            "EMBED0001",
            "Unknown error",
            "Unknown error",
            "EmbedderGenerator",
            DiagnosticSeverity.Warning,
            true);
        public static readonly DiagnosticDescriptor EMBED0002_OlderVersion = new(
            "EMBED0002",
            "embeder version is older",
            "embeder version({0}) is older than {1}({2})",
            "EmbedderGenerator",
            DiagnosticSeverity.Warning,
            true);
        public static readonly DiagnosticDescriptor EMBED0003_ParseConfigError = new(
            "EMBED0003",
            "Error config file",
            "Error config file: {0}",
            "EmbedderGenerator",
            DiagnosticSeverity.Error,
            true);
        public static readonly DiagnosticDescriptor EMBED0004_ErrorEmbeddedSource = new(
            "EMBED0004",
            "Error embedded source",
            "Error embedded source: {0}",
            "EmbedderGenerator",
            DiagnosticSeverity.Warning,
            true);
        public static readonly DiagnosticDescriptor EMBED0005_EmbeddedSourceDiff = new(
            "EMBED0005",
            "Different syntax",
            "Different syntax: near {0}. This is Embedder error, please report this to GitHub repository.",
            "EmbedderGenerator",
            DiagnosticSeverity.Error,
            true);
    }
}
