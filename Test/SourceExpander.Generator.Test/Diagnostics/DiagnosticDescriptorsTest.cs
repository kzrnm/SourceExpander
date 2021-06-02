﻿using System;
using System.Globalization;
using FluentAssertions;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace SourceExpander.Diagnostics
{
    public class DiagnosticDescriptorsTest
    {
        static readonly IFormatProvider formatProvider = CultureInfo.InvariantCulture;
        [Fact]
        public void EXPAND0001()
        {
            DiagnosticDescriptors.EXPAND0001_UnknownError("LX")
                .GetMessage(formatProvider)
                .Should()
                .Be("Unknown error: LX");
        }
        [Fact]
        public void EXPAND0002()
        {
            DiagnosticDescriptors.EXPAND0002_ExpanderVersion(new Version(2, 0, 0), "Newerlib", new Version(3, 0, 0))
                .GetMessage(formatProvider)
                .Should()
                .Be("Expander version(2.0.0) is older than embedder of Newerlib(3.0.0)");
        }
        [Fact]
        public void EXPAND0003()
        {
            DiagnosticDescriptors.EXPAND0003_NotFoundEmbedded()
                .GetMessage(formatProvider)
                .Should()
                .Be("Not found embedded source");
        }
        [Fact]
        public void EXPAND0004()
        {
            DiagnosticDescriptors.EXPAND0004_MustBeNewerThanCSharp3()
                .GetMessage(formatProvider)
                .Should()
                .Be("Need C# 3 or later");
        }
        [Fact]
        public void EXPAND0005()
        {
            DiagnosticDescriptors.EXPAND0005_NewerCSharpVersion(LanguageVersion.CSharp7, "Newerlib", LanguageVersion.CSharp8)
                .GetMessage(formatProvider)
                .Should()
                .Be("C# version(7.0) is older than embedded Newerlib(8.0)");
        }
        [Fact]
        public void EXPAND0006()
        {
            DiagnosticDescriptors.EXPAND0006_AllowUnsafe("Unsafelib")
                .GetMessage(formatProvider)
                .Should()
                .Be("Needs AllowUnsafeBlocks because Unsafelib has AllowUnsafeBlocks");
        }
        [Fact]
        public void EXPAND0007()
        {
            DiagnosticDescriptors.EXPAND0007_ParseConfigError("/home/source/SourceExpander.Generator.Config.json", "any error")
                .GetMessage(formatProvider)
                .Should()
                .Be("Error config file: Path: /home/source/SourceExpander.Generator.Config.json, Message: any error");
        }
        [Fact]
        public void EXPAND0008()
        {
            DiagnosticDescriptors.EXPAND0008_EmbeddedDataError("Anotherlib", "SourceExpander.EmbeddedSourceCode", "There was an error deserializing the object of type SourceExpander.SourceFileInfo[]. Encountered unexpected character '}'.")
                .GetMessage(formatProvider)
                .Should()
                .Be("Invalid embedded data: Anotherlib, Key: SourceExpander.EmbeddedSourceCode, Message: There was an error deserializing the object of type SourceExpander.SourceFileInfo[]. Encountered unexpected character '}'.");
        }
        [Fact]
        public void EXPAND0009()
        {
            DiagnosticDescriptors.EXPAND0009_MetadataEmbeddingFileNotFound("Program.cs")
                .GetMessage(formatProvider)
                .Should()
                .Be("MetadataEmbeddingFile is not found: name: Program.cs");
        }
    }
}
