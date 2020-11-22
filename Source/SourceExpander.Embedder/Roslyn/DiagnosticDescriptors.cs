﻿using Microsoft.CodeAnalysis;
#pragma warning disable RS2008
namespace SourceExpander.Roslyn
{
    public static class DiagnosticDescriptors
    {
        public static readonly DiagnosticDescriptor EMBED0001 = new DiagnosticDescriptor(
            "EMBED0001",
            "embeder version is older",
            "embeder version({0}) is older than {1}({2})",
            "EmbedderGenerator",
            DiagnosticSeverity.Warning,
            true);
    }
}
