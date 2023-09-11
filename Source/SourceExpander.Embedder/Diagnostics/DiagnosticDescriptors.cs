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

        public static Diagnostic EMBED0001_UnknownError(string message)
            => Diagnostic.Create(EMBED0001_UnknownError_Descriptor, Location.None, message);
        private static readonly DiagnosticDescriptor EMBED0001_UnknownError_Descriptor = new DiagnosticDescriptor(
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
        public static Diagnostic EMBED0002_OlderVersion(Version embbederVersion, string assemblyName, Version assemblyEmbedderVersion)
            => Diagnostic.Create(EMBED0002_OlderVersion_Descriptor, Location.None, embbederVersion, assemblyName, assemblyEmbedderVersion);
        private static readonly DiagnosticDescriptor EMBED0002_OlderVersion_Descriptor = new DiagnosticDescriptor(
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
        public static Diagnostic EMBED0003_ParseConfigError(string? configFile, string message)
            => Diagnostic.Create(EMBED0003_ParseConfigError_Descriptor,
                AdditionalFileLocation(configFile), configFile, message);
        private static readonly DiagnosticDescriptor EMBED0003_ParseConfigError_Descriptor = new DiagnosticDescriptor(
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
        public static Diagnostic EMBED0004_ErrorEmbeddedSource(string file, string message)
            => Diagnostic.Create(EMBED0004_ErrorEmbeddedSource_Descriptor, Location.None, file, message);
        private static readonly DiagnosticDescriptor EMBED0004_ErrorEmbeddedSource_Descriptor = new DiagnosticDescriptor(
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
        public static Diagnostic EMBED0005_EmbeddedSourceDiff(string diffString)
            => Diagnostic.Create(EMBED0005_EmbeddedSourceDiff_Descriptor, Location.None, diffString);
        private static readonly DiagnosticDescriptor EMBED0005_EmbeddedSourceDiff_Descriptor = new DiagnosticDescriptor(
            "EMBED0005",
            new LocalizableResourceString(
                nameof(DiagnosticsResources.EMBED0005_Title),
                DiagnosticsResources.ResourceManager,
                typeof(DiagnosticsResources)),
            new LocalizableResourceString(
                nameof(DiagnosticsResources.EMBED0005_Body),
                DiagnosticsResources.ResourceManager,
                typeof(DiagnosticsResources)),
            "Error",
            DiagnosticSeverity.Error,
            true);
        public static Diagnostic EMBED0006_AnotherAssemblyEmbeddedDataError(string? assemblyName, string key, string message)
            => Diagnostic.Create(EMBED0006_AnotherAssemblyEmbeddedDataError_Descriptor, Location.None, assemblyName, key, message);
        private static readonly DiagnosticDescriptor EMBED0006_AnotherAssemblyEmbeddedDataError_Descriptor = new DiagnosticDescriptor(
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
        public static Diagnostic EMBED0007_NullableProject()
            => Diagnostic.Create(EMBED0007_NullableProject_Descriptor, Location.None);
        private static readonly DiagnosticDescriptor EMBED0007_NullableProject_Descriptor = new DiagnosticDescriptor(
            "EMBED0007",
            new LocalizableResourceString(
                nameof(DiagnosticsResources.EMBED0007_Title),
                DiagnosticsResources.ResourceManager,
                typeof(DiagnosticsResources)),
            new LocalizableResourceString(
                nameof(DiagnosticsResources.EMBED0007_Title),
                DiagnosticsResources.ResourceManager,
                typeof(DiagnosticsResources)),
            "Usage",
            DiagnosticSeverity.Warning,
            true);
        public static Diagnostic EMBED0008_NullableDirective(Location location)
            => Diagnostic.Create(EMBED0008_NullableDirective_Descriptor, location);
        private static readonly DiagnosticDescriptor EMBED0008_NullableDirective_Descriptor = new DiagnosticDescriptor(
            "EMBED0008",
            new LocalizableResourceString(
                nameof(DiagnosticsResources.EMBED0008_Title),
                DiagnosticsResources.ResourceManager,
                typeof(DiagnosticsResources)),
            new LocalizableResourceString(
                nameof(DiagnosticsResources.EMBED0008_Title),
                DiagnosticsResources.ResourceManager,
                typeof(DiagnosticsResources)),
            "Usage",
            DiagnosticSeverity.Warning,
            true);
        public static Diagnostic EMBED0009_UsingStaticDirective(Location location)
            => Diagnostic.Create(EMBED0009_UsingStaticDirective_Descriptor, location);
        private static readonly DiagnosticDescriptor EMBED0009_UsingStaticDirective_Descriptor = new DiagnosticDescriptor(
            "EMBED0009",
            new LocalizableResourceString(
                nameof(DiagnosticsResources.EMBED0009_Title),
                DiagnosticsResources.ResourceManager,
                typeof(DiagnosticsResources)),
            new LocalizableResourceString(
                nameof(DiagnosticsResources.EMBED0009_Body),
                DiagnosticsResources.ResourceManager,
                typeof(DiagnosticsResources)),
            "Usage",
            DiagnosticSeverity.Info,
            true);
        public static Diagnostic EMBED0010_UsingAliasDirective(Location location)
            => Diagnostic.Create(EMBED0010_UsingAliasDirective_Descriptor, location);
        private static readonly DiagnosticDescriptor EMBED0010_UsingAliasDirective_Descriptor = new DiagnosticDescriptor(
            "EMBED0010",
            new LocalizableResourceString(
                nameof(DiagnosticsResources.EMBED0010_Title),
                DiagnosticsResources.ResourceManager,
                typeof(DiagnosticsResources)),
            new LocalizableResourceString(
                nameof(DiagnosticsResources.EMBED0010_Body),
                DiagnosticsResources.ResourceManager,
                typeof(DiagnosticsResources)),
            "Usage",
            DiagnosticSeverity.Info,
            true);


        public static Diagnostic EMBED0011_ObsoleteConfigProperty(
            string? configFile, string obsoleteProperty, string insteadProperty)
            => Diagnostic.Create(EMBED0011_ObsoleteConfigProperty_Descriptor,
                AdditionalFileLocation(configFile), configFile ?? "Any of configs", obsoleteProperty, insteadProperty);
        private static readonly DiagnosticDescriptor EMBED0011_ObsoleteConfigProperty_Descriptor = new DiagnosticDescriptor(
            "EMBED0011",
            new LocalizableResourceString(
                nameof(DiagnosticsResources.EMBED0011_Title),
                DiagnosticsResources.ResourceManager,
                typeof(DiagnosticsResources)),
            new LocalizableResourceString(
                nameof(DiagnosticsResources.EMBED0011_Body),
                DiagnosticsResources.ResourceManager,
                typeof(DiagnosticsResources)),
            "Config",
            DiagnosticSeverity.Warning,
            true);
    }
}
