using System;
using System.Globalization;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace SourceExpander.Generator.Diagnostics.Test
{
    public class DiagnosticDescriptorsTest
    {
        static readonly IFormatProvider formatProvider = CultureInfo.InvariantCulture;
        [Fact]
        public void EXPAND0001()
        {
            Diagnostic.Create(DiagnosticDescriptors.EXPAND0001_UnknownError, Location.None, "LX")
                .GetMessage(formatProvider)
                .Should()
                .Be("Unknown error: LX");
        }
        [Fact]
        public void EXPAND0002()
        {
            Diagnostic.Create(DiagnosticDescriptors.EXPAND0002_ExpanderVersion, Location.None, new Version(2, 0, 0).ToString(), "Newerlib", new Version(3, 0, 0).ToString())
                .GetMessage(formatProvider)
                .Should()
                .Be("Expander version(2.0.0) is older than embedder of Newerlib(3.0.0)");
        }
        [Fact]
        public void EXPAND0003()
        {
            Diagnostic.Create(DiagnosticDescriptors.EXPAND0003_NotFoundEmbedded, Location.None)
                .GetMessage(formatProvider)
                .Should()
                .Be("Not found embedded source");
        }
        [Fact]
        public void EXPAND0004()
        {
            Diagnostic.Create(DiagnosticDescriptors.EXPAND0004_MustBeNewerThanCSharp3, Location.None)
                .GetMessage(formatProvider)
                .Should()
                .Be("Need C# 3 or later");
        }
        [Fact]
        public void EXPAND0005()
        {
            Diagnostic.Create(DiagnosticDescriptors.EXPAND0005_NewerCSharpVersion, Location.None, LanguageVersion.CSharp7.ToDisplayString(), "Newerlib", LanguageVersion.CSharp8.ToDisplayString())
                .GetMessage(formatProvider)
                .Should()
                .Be("C# version(7.0) is older than embedded Newerlib(8.0)");
        }
        [Fact]
        public void EXPAND0006()
        {
            Diagnostic.Create(DiagnosticDescriptors.EXPAND0006_AllowUnsafe, Location.None, "Unsafelib")
                .GetMessage(formatProvider)
                .Should()
                .Be("Needs AllowUnsafeBlocks because Unsafelib has AllowUnsafeBlocks");
        }
        [Fact]
        public void EXPAND0007()
        {
            Diagnostic.Create(DiagnosticDescriptors.EXPAND0007_ParseConfigError, Location.None, "/home/source/SourceExpander.Generator.Config.json")
                .GetMessage(formatProvider)
                .Should()
                .Be("Error config file: /home/source/SourceExpander.Generator.Config.json");
        }
        [Fact]
        public void EXPAND0008()
        {
            Diagnostic.Create(DiagnosticDescriptors.EXPAND0008_EmbeddedDataError, Location.None, "Anotherlib", "SourceExpander.EmbeddedSourceCode", "There was an error deserializing the object of type SourceExpander.SourceFileInfo[]. Encountered unexpected character '}'.")
                .GetMessage(formatProvider)
                .Should()
                .Be("Invalid embedded data: Anotherlib, Key: SourceExpander.EmbeddedSourceCode, Message: There was an error deserializing the object of type SourceExpander.SourceFileInfo[]. Encountered unexpected character '}'.");
        }
    }
}
