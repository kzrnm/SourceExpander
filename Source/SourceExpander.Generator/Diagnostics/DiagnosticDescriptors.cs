﻿using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SourceExpander.Diagnostics;

namespace SourceExpander
{
    public static class DiagnosticDescriptors
    {
        private static Location AdditionalFileLocation(string? filePath) => filePath switch
        {
            null => Location.None,
            _ => Location.Create(filePath, new(), new()),
        };

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
        public static Diagnostic EXPAND0007_ParseConfigError(string? configFile, string message)
             => Diagnostic.Create(EXPAND0007_ParseConfigError_Descriptor,
                 AdditionalFileLocation(configFile), configFile ?? "Any of configs", message);
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
        public static Diagnostic EXPAND0009_MetadataEmbeddingFileNotFound(string fileName)
             => Diagnostic.Create(EXPAND0009_MetadataEmbeddingFileNotFound_Descriptor, Location.None, fileName);
        private static readonly DiagnosticDescriptor EXPAND0009_MetadataEmbeddingFileNotFound_Descriptor = new(
            "EXPAND0009",
            new LocalizableResourceString(
                nameof(DiagnosticsResources.EXPAND0009_Title),
                DiagnosticsResources.ResourceManager,
                typeof(DiagnosticsResources)),
            new LocalizableResourceString(
                nameof(DiagnosticsResources.EXPAND0009_Body),
                DiagnosticsResources.ResourceManager,
                typeof(DiagnosticsResources)),
            "Error",
            DiagnosticSeverity.Warning,
            true);
        public static Diagnostic EXPAND0010_UnsafeBlock(string fileName)
             => Diagnostic.Create(EXPAND0010_UnsafeBlock_Descriptor, Location.None, fileName);
        private static readonly DiagnosticDescriptor EXPAND0010_UnsafeBlock_Descriptor = new(
            "EXPAND0010",
            new LocalizableResourceString(
                nameof(DiagnosticsResources.EXPAND0010_Title),
                DiagnosticsResources.ResourceManager,
                typeof(DiagnosticsResources)),
            new LocalizableResourceString(
                nameof(DiagnosticsResources.EXPAND0010_Body),
                DiagnosticsResources.ResourceManager,
                typeof(DiagnosticsResources)),
            "Usage",
            DiagnosticSeverity.Warning,
            true);
    }
}
