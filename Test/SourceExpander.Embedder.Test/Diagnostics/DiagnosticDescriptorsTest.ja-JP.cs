using System;
using System.Globalization;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Xunit;

namespace SourceExpander.Embedder.Diagnostics.Test
{
    public class DiagnosticDescriptorsTestJaJP
    {
        static readonly IFormatProvider formatProvider = new CultureInfo("ja-JP");
        [Fact]
        public void EMBED0001()
        {
            Diagnostic.Create(DiagnosticDescriptors.EMBED0001_UnknownError, Location.None, "LX")
                .GetMessage(formatProvider)
                .Should()
                .Be("不明なエラー: LX");
        }
        [Fact]
        public void EMBED0002()
        {
            Diagnostic.Create(DiagnosticDescriptors.EMBED0002_OlderVersion, Location.None, new Version(2, 0, 0).ToString(), "Newerlib", new Version(3, 0, 0).ToString())
                .GetMessage(formatProvider)
                .Should()
                .Be("Embeder version(2.0.0) が Newerlib(3.0.0) の Embedder より古いです");
        }
        [Fact]
        public void EMBED0003()
        {
            Diagnostic.Create(DiagnosticDescriptors.EMBED0003_ParseConfigError, Location.None, "/home/source/SourceExpander.Embedder.Config.json")
                .GetMessage(formatProvider)
                .Should()
                .Be("Error config file: /home/source/SourceExpander.Embedder.Config.json");
        }
        [Fact]
        public void EMBED0004()
        {
            Diagnostic.Create(DiagnosticDescriptors.EMBED0004_ErrorEmbeddedSource, Location.None, "P.cs", "any error")
                .GetMessage(formatProvider)
                .Should()
                .Be("Error embedded source: File: P.cs, Message: any error");
        }
        [Fact]
        public void EMBED0005()
        {
            Diagnostic.Create(DiagnosticDescriptors.EMBED0005_EmbeddedSourceDiff, Location.None, "Uns")
                .GetMessage(formatProvider)
                .Should()
                .Be("Different syntax: near Uns. This is Embedder error, please report this to GitHub repository.");
        }
        [Fact]
        public void EMBED0006()
        {
            Diagnostic.Create(DiagnosticDescriptors.EMBED0006_AnotherAssemblyEmbeddedDataError, Location.None, "Other", "SourceExpander.EmbeddedSourceCode", "There was an error deserializing the object of type SourceExpander.SourceFileInfo[]. Encountered unexpected character '}'.")
                .GetMessage(formatProvider)
                .Should()
                .Be("他のアセンブリの埋め込みデータが不正です: Other, Key: SourceExpander.EmbeddedSourceCode, Message: There was an error deserializing the object of type SourceExpander.SourceFileInfo[]. Encountered unexpected character '}'.");
        }
    }
}
