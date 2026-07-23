using System;
using Microsoft.CodeAnalysis;
using SourceExpander.Diagnostics;

namespace SourceExpander
{
    public static class DiagnosticDescriptors
    {
        public static Location AdditionalFileLocation(string? filePath) => filePath switch
        {
            null => Location.None,
            _ => Location.Create(filePath, new(), new()),
        };

        static LocalizableResourceString ResourceString(string name)
            => new(name, DiagnosticsResources.ResourceManager, typeof(DiagnosticsResources));

        public static Diagnostic EMBED0001_UnknownError(string message)
            => Diagnostic.Create(EMBED0001_UnknownError_Descriptor, Location.None, message);
        private static readonly DiagnosticDescriptor EMBED0001_UnknownError_Descriptor = new(
            "EMBED0001",
            ResourceString(nameof(DiagnosticsResources.EMBED0001_Title)),
            ResourceString(nameof(DiagnosticsResources.EMBED0001_Body)),
            "Error",
            DiagnosticSeverity.Warning,
            true);
        public static Diagnostic EMBED0002_OlderVersion(Version embbederVersion, string assemblyName, Version assemblyEmbedderVersion)
            => Diagnostic.Create(EMBED0002_OlderVersion_Descriptor, Location.None, embbederVersion, assemblyName, assemblyEmbedderVersion);
        private static readonly DiagnosticDescriptor EMBED0002_OlderVersion_Descriptor = new(
            "EMBED0002",
            ResourceString(nameof(DiagnosticsResources.EMBED0002_Title)),
            ResourceString(nameof(DiagnosticsResources.EMBED0002_Body)),
            "Usage",
            DiagnosticSeverity.Warning,
            true);
        public static Diagnostic EMBED0003_ParseConfigError(string? configFile, string message)
            => Diagnostic.Create(EMBED0003_ParseConfigError_Descriptor,
                AdditionalFileLocation(configFile), configFile, message);
        private static readonly DiagnosticDescriptor EMBED0003_ParseConfigError_Descriptor = new(
            "EMBED0003",
            ResourceString(nameof(DiagnosticsResources.EMBED0003_Title)),
            ResourceString(nameof(DiagnosticsResources.EMBED0003_Body)),
            "Error",
            DiagnosticSeverity.Error,
            true);
        public static Diagnostic EMBED0004_ErrorEmbeddedSource(string file, string message)
            => Diagnostic.Create(EMBED0004_ErrorEmbeddedSource_Descriptor, Location.None, file, message);
        private static readonly DiagnosticDescriptor EMBED0004_ErrorEmbeddedSource_Descriptor = new(
            "EMBED0004",
            ResourceString(nameof(DiagnosticsResources.EMBED0004_Title)),
            ResourceString(nameof(DiagnosticsResources.EMBED0004_Body)),
            "Error",
            DiagnosticSeverity.Warning,
            true);
        public static Diagnostic EMBED0005_EmbeddedSourceDiff(string diffString)
            => Diagnostic.Create(EMBED0005_EmbeddedSourceDiff_Descriptor, Location.None, diffString);
        private static readonly DiagnosticDescriptor EMBED0005_EmbeddedSourceDiff_Descriptor = new(
            "EMBED0005",
            ResourceString(nameof(DiagnosticsResources.EMBED0005_Title)),
            ResourceString(nameof(DiagnosticsResources.EMBED0005_Body)),
            "Error",
            DiagnosticSeverity.Error,
            true);
        public static Diagnostic EMBED0006_AnotherAssemblyEmbeddedDataError(string? assemblyName, string key, string message)
            => Diagnostic.Create(EMBED0006_AnotherAssemblyEmbeddedDataError_Descriptor, Location.None, assemblyName, key, message);
        private static readonly DiagnosticDescriptor EMBED0006_AnotherAssemblyEmbeddedDataError_Descriptor = new(
            "EMBED0006",
            ResourceString(nameof(DiagnosticsResources.EMBED0006_Title)),
            ResourceString(nameof(DiagnosticsResources.EMBED0006_Body)),
            "Error",
            DiagnosticSeverity.Warning,
            true);
        public static Diagnostic EMBED0009_UsingStaticDirective(Location location)
            => Diagnostic.Create(EMBED0009_UsingStaticDirective_Descriptor, location);
        private static readonly DiagnosticDescriptor EMBED0009_UsingStaticDirective_Descriptor = new(
            "EMBED0009",
            ResourceString(nameof(DiagnosticsResources.EMBED0009_Title)),
            ResourceString(nameof(DiagnosticsResources.EMBED0009_Body)),
            "Usage",
            DiagnosticSeverity.Info,
            true);
        public static Diagnostic EMBED0010_UsingAliasDirective(Location location)
            => Diagnostic.Create(EMBED0010_UsingAliasDirective_Descriptor, location);
        private static readonly DiagnosticDescriptor EMBED0010_UsingAliasDirective_Descriptor = new(
            "EMBED0010",
            ResourceString(nameof(DiagnosticsResources.EMBED0010_Title)),
            ResourceString(nameof(DiagnosticsResources.EMBED0010_Body)),
            "Usage",
            DiagnosticSeverity.Info,
            true);

        public static Diagnostic EMBED0011_ObsoleteConfigProperty(
            string? configFile, string obsoleteProperty, string insteadProperty)
            => Diagnostic.Create(EMBED0011_ObsoleteConfigProperty_Descriptor,
                AdditionalFileLocation(configFile), configFile ?? "Any of configs", obsoleteProperty, insteadProperty);
        private static readonly DiagnosticDescriptor EMBED0011_ObsoleteConfigProperty_Descriptor = new(
            "EMBED0011",
            ResourceString(nameof(DiagnosticsResources.EMBED0011_Title)),
            ResourceString(nameof(DiagnosticsResources.EMBED0011_Body)),
            "Config",
            DiagnosticSeverity.Warning,
            true);

        public static Diagnostic EMBED0012_InvalidAttribute(Location location, string attributeName)
            => Diagnostic.Create(EMBED0012_InvalidAttribute_Descriptor, location, attributeName);
        private static readonly DiagnosticDescriptor EMBED0012_InvalidAttribute_Descriptor = new(
            "EMBED0012",
            ResourceString(nameof(DiagnosticsResources.EMBED0012_Title)),
            ResourceString(nameof(DiagnosticsResources.EMBED0012_Body)),
            "Usage",
            DiagnosticSeverity.Warning,
            true);

        public static Diagnostic EMBED0013_InvalidEmbeddedData(string fileName)
             => Diagnostic.Create(EMBED0013_InvalidEmbeddedData_Descriptor, Location.None, fileName);
        private static readonly DiagnosticDescriptor EMBED0013_InvalidEmbeddedData_Descriptor = new(
            "EMBED0013",
            ResourceString(nameof(DiagnosticsResources.EMBED0013_Title)),
            ResourceString(nameof(DiagnosticsResources.EMBED0013_Body)),
            "Error",
            DiagnosticSeverity.Warning,
            true);
    }
}
