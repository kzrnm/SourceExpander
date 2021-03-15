﻿using Microsoft.CodeAnalysis;
using SourceExpander.Diagnostics;

namespace SourceExpander
{
    public static class DiagnosticDescriptors
    {
        public static readonly DiagnosticDescriptor EMBED0001_UnknownError = new(
            "EMBED0001",
            new LocalizableResourceString(
                nameof(DiagnosticsResources.EMBED0001_Title),
                DiagnosticsResources.ResourceManager,
                typeof(DiagnosticsResources)),
            new LocalizableResourceString(
                nameof(DiagnosticsResources.EMBED0001_Body),
                DiagnosticsResources.ResourceManager,
                typeof(DiagnosticsResources)),
            "Error",
            DiagnosticSeverity.Warning,
            true);
        public static readonly DiagnosticDescriptor EMBED0002_OlderVersion = new(
            "EMBED0002",
            new LocalizableResourceString(
                nameof(DiagnosticsResources.EMBED0002_Title),
                DiagnosticsResources.ResourceManager,
                typeof(DiagnosticsResources)),
            new LocalizableResourceString(
                nameof(DiagnosticsResources.EMBED0002_Body),
                DiagnosticsResources.ResourceManager,
                typeof(DiagnosticsResources)),
            "Usage",
            DiagnosticSeverity.Warning,
            true);
        public static readonly DiagnosticDescriptor EMBED0003_ParseConfigError = new(
            "EMBED0003",
            new LocalizableResourceString(
                nameof(DiagnosticsResources.EMBED0003_Title),
                DiagnosticsResources.ResourceManager,
                typeof(DiagnosticsResources)),
            new LocalizableResourceString(
                nameof(DiagnosticsResources.EMBED0003_Body),
                DiagnosticsResources.ResourceManager,
                typeof(DiagnosticsResources)),
            "Error",
            DiagnosticSeverity.Error,
            true);
        public static readonly DiagnosticDescriptor EMBED0004_ErrorEmbeddedSource = new(
            "EMBED0004",
            new LocalizableResourceString(
                nameof(DiagnosticsResources.EMBED0004_Title),
                DiagnosticsResources.ResourceManager,
                typeof(DiagnosticsResources)),
            new LocalizableResourceString(
                nameof(DiagnosticsResources.EMBED0004_Body),
                DiagnosticsResources.ResourceManager,
                typeof(DiagnosticsResources)),
            "Error",
            DiagnosticSeverity.Warning,
            true);
        public static readonly DiagnosticDescriptor EMBED0005_EmbeddedSourceDiff = new(
            "EMBED0005",
            new LocalizableResourceString(
                nameof(DiagnosticsResources.EMBED0005_Title),
                DiagnosticsResources.ResourceManager,
                typeof(DiagnosticsResources)),
            new LocalizableResourceString(
                nameof(DiagnosticsResources.EMBED0005_Body),
                DiagnosticsResources.ResourceManager,
                typeof(DiagnosticsResources)),
            "EmbedderError",
            DiagnosticSeverity.Error,
            true);
        public static readonly DiagnosticDescriptor EMBED0006_AnotherAssemblyEmbeddedDataError = new(
            "EMBED0006",
            new LocalizableResourceString(
                nameof(DiagnosticsResources.EMBED0006_Title),
                DiagnosticsResources.ResourceManager,
                typeof(DiagnosticsResources)),
            new LocalizableResourceString(
                nameof(DiagnosticsResources.EMBED0006_Body),
                DiagnosticsResources.ResourceManager,
                typeof(DiagnosticsResources)),
            "Error",
            DiagnosticSeverity.Warning,
            true);
    }
}
