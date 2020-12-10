using Microsoft.CodeAnalysis;
#pragma warning disable RS2008
namespace SourceExpander.Roslyn
{
    public static class DiagnosticDescriptors
    {
        public static readonly DiagnosticDescriptor EMBED0001_OlderVersion = new DiagnosticDescriptor(
            "EMBED0001",
            "embeder version is older",
            "embeder version({0}) is older than {1}({2})",
            "EmbedderGenerator",
            DiagnosticSeverity.Warning,
            true);
        public static readonly DiagnosticDescriptor EMBED0002_NotCSharp = new DiagnosticDescriptor(
            "EMBED0002",
            "Compilation must be C#",
            "Compilation must be C#",
            "EmbedderGenerator",
            DiagnosticSeverity.Error,
            true);
        public static readonly DiagnosticDescriptor EMBED0003_ParseConfigError = new DiagnosticDescriptor(
            "EMBED0003",
            "Error config file",
            "Error config file: {0}",
            "EmbedderGenerator",
            DiagnosticSeverity.Error,
            true);
    }
}
