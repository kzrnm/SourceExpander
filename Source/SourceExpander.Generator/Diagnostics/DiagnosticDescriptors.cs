using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SourceExpander.Diagnostics;

namespace SourceExpander
{
    public static class DiagnosticDescriptors
    {
        public static Diagnostic EXPAND0001_UnknownError(string message)
             => Diagnostic.Create(EXPAND0001_UnknownError_Descriptor, Location.None, message);
        private static readonly DiagnosticDescriptor EXPAND0001_UnknownError_Descriptor = new(
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
        public static Diagnostic EXPAND0002_ExpanderVersion(Version expanderVersion, string assemblyName, Version assemblyEmbedderVersion)
             => Diagnostic.Create(EXPAND0002_ExpanderVersion_Descriptor, Location.None, expanderVersion.ToString(), assemblyName, assemblyEmbedderVersion.ToString());
        private static readonly DiagnosticDescriptor EXPAND0002_ExpanderVersion_Descriptor = new(
            "EXPAND0002",
            new LocalizableResourceString(
                nameof(DiagnosticsResources.EXPAND0002_Title),
                DiagnosticsResources.ResourceManager,
                typeof(DiagnosticsResources)),
            new LocalizableResourceString(
                nameof(DiagnosticsResources.EXPAND0002_Body),
                DiagnosticsResources.ResourceManager,
                typeof(DiagnosticsResources)),
            "Usage",
            DiagnosticSeverity.Warning,
            true);
        public static Diagnostic EXPAND0003_NotFoundEmbedded()
             => Diagnostic.Create(EXPAND0003_NotFoundEmbedded_Descriptor, Location.None);
        private static readonly DiagnosticDescriptor EXPAND0003_NotFoundEmbedded_Descriptor = new(
            "EXPAND0003",
            new LocalizableResourceString(
                nameof(DiagnosticsResources.EXPAND0003_Title),
                DiagnosticsResources.ResourceManager,
                typeof(DiagnosticsResources)),
            new LocalizableResourceString(
                nameof(DiagnosticsResources.EXPAND0003_Title),
                DiagnosticsResources.ResourceManager,
                typeof(DiagnosticsResources)),
            "Usage",
            DiagnosticSeverity.Info,
            true);
        public static Diagnostic EXPAND0004_MustBeNewerThanCSharp3()
             => Diagnostic.Create(EXPAND0004_MustBeNewerThanCSharp3_Descriptor, Location.None);
        private static readonly DiagnosticDescriptor EXPAND0004_MustBeNewerThanCSharp3_Descriptor = new(
            "EXPAND0004",
            new LocalizableResourceString(
                nameof(DiagnosticsResources.EXPAND0004_Title),
                DiagnosticsResources.ResourceManager,
                typeof(DiagnosticsResources)),
            new LocalizableResourceString(
                nameof(DiagnosticsResources.EXPAND0004_Title),
                DiagnosticsResources.ResourceManager,
                typeof(DiagnosticsResources)),
            "Usage",
            DiagnosticSeverity.Info,
            true);
        public static Diagnostic EXPAND0005_NewerCSharpVersion(LanguageVersion expanderVersion, string assemblyName, LanguageVersion assemblyEmbedderVersion)
             => Diagnostic.Create(EXPAND0005_NewerCSharpVersion_Descriptor, Location.None, expanderVersion.ToDisplayString(), assemblyName, assemblyEmbedderVersion.ToDisplayString());
        private static readonly DiagnosticDescriptor EXPAND0005_NewerCSharpVersion_Descriptor = new(
            "EXPAND0005",
            new LocalizableResourceString(
                nameof(DiagnosticsResources.EXPAND0005_Title),
                DiagnosticsResources.ResourceManager,
                typeof(DiagnosticsResources)),
            new LocalizableResourceString(
                nameof(DiagnosticsResources.EXPAND0005_Body),
                DiagnosticsResources.ResourceManager,
                typeof(DiagnosticsResources)),
            "Usage",
            DiagnosticSeverity.Warning,
            true);
        public static Diagnostic EXPAND0006_AllowUnsafe(string assemblyName)
             => Diagnostic.Create(EXPAND0006_AllowUnsafe_Descriptor, Location.None, assemblyName);
        private static readonly DiagnosticDescriptor EXPAND0006_AllowUnsafe_Descriptor = new(
            "EXPAND0006",
            new LocalizableResourceString(
                nameof(DiagnosticsResources.EXPAND0006_Title),
                DiagnosticsResources.ResourceManager,
                typeof(DiagnosticsResources)),
            new LocalizableResourceString(
                nameof(DiagnosticsResources.EXPAND0006_Body),
                DiagnosticsResources.ResourceManager,
                typeof(DiagnosticsResources)),
            "Usage",
            DiagnosticSeverity.Warning,
            true);
        public static Diagnostic EXPAND0007_ParseConfigError(string configFile, string message)
             => Diagnostic.Create(EXPAND0007_ParseConfigError_Descriptor, Location.None, configFile, message);
        private static readonly DiagnosticDescriptor EXPAND0007_ParseConfigError_Descriptor = new(
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
        public static Diagnostic EXPAND0008_EmbeddedDataError(string? assemblyName, string key, string value)
             => Diagnostic.Create(EXPAND0008_EmbeddedDataError_Descriptor, Location.None, assemblyName, key, value);
        private static readonly DiagnosticDescriptor EXPAND0008_EmbeddedDataError_Descriptor = new(
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
