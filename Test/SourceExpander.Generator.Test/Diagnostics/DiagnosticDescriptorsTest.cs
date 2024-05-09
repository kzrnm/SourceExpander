using System;
using System.Globalization;
using FluentAssertions;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace SourceExpander.Diagnostics
{
    public class DiagnosticDescriptorsTest : DiagnosticDescriptorsTestBase
    {
        protected override CultureInfo FormatProvider => CultureInfo.InvariantCulture;
    }
    public class DiagnosticDescriptorsTestJaJp : DiagnosticDescriptorsTestBase
    {
        protected override CultureInfo FormatProvider => new("ja-JP");
    }
    public abstract class DiagnosticDescriptorsTestBase
    {
        protected abstract CultureInfo FormatProvider { get; }
        [Fact]
        public void EXPAND0001()
        {
            DiagnosticDescriptors.EXPAND0001_UnknownError("LX")
                .GetMessage(FormatProvider)
                .Should()
                .Be(FormatProvider.Name switch
                {
                    "ja-JP" => "不明なエラー: LX",
                    _ => "Unknown error: LX",
                });
        }
        [Fact]
        public void EXPAND0002()
        {
            DiagnosticDescriptors.EXPAND0002_ExpanderVersion(new Version(2, 0, 0), "Newerlib", new Version(3, 0, 0))
                .GetMessage(FormatProvider)
                .Should()
                .Be(FormatProvider.Name switch
                {
                    "ja-JP" => "Expander version(2.0.0) が Newerlib(3.0.0) の embedder より古いです",
                    _ => "Expander version(2.0.0) is older than embedder of Newerlib(3.0.0)",
                });
        }
        [Fact]
        public void EXPAND0003()
        {
            DiagnosticDescriptors.EXPAND0003_NotFoundEmbedded()
                .GetMessage(FormatProvider)
                .Should()
                .Be(FormatProvider.Name switch
                {
                    "ja-JP" => "埋め込みソースが見つかりません",
                    _ => "Not found embedded source",
                });
        }
        [Fact]
        public void EXPAND0004()
        {
            DiagnosticDescriptors.EXPAND0004_MustBeNewerThanCSharp3()
                .GetMessage(FormatProvider)
                .Should()
                .Be(FormatProvider.Name switch
                {
                    _ => "Need C# 3 or later",
                });
        }
        [Fact]
        public void EXPAND0005()
        {
            DiagnosticDescriptors.EXPAND0005_NewerCSharpVersion(LanguageVersion.CSharp7, "Newerlib", LanguageVersion.CSharp8)
                .GetMessage(FormatProvider)
                .Should()
                .Be(FormatProvider.Name switch
                {
                    "ja-JP" => "C# のバージョン(7.0) が埋め込まれている Newerlib(8.0) より古いです。",
                    _ => "C# version(7.0) is older than embedded Newerlib(8.0)",
                });
        }
        [Fact]
        public void EXPAND0006()
        {
            DiagnosticDescriptors.EXPAND0006_AllowUnsafe("Unsafelib")
                .GetMessage(FormatProvider)
                .Should()
                .Be(FormatProvider.Name switch
                {
                    "ja-JP" => "Unsafelib が AllowUnsafeBlocks を持っているので AllowUnsafeBlocks が必要です",
                    _ => "Needs AllowUnsafeBlocks because Unsafelib has AllowUnsafeBlocks",
                });
        }
        [Fact]
        public void EXPAND0007()
        {
            DiagnosticDescriptors.EXPAND0007_ParseConfigError("/home/source/SourceExpander.Generator.Config.json", "any error")
                .GetMessage(FormatProvider)
                .Should()
                .Be(FormatProvider.Name switch
                {
                    _ => "Error config file: Path: /home/source/SourceExpander.Generator.Config.json, Message: any error",
                });
        }
        [Fact]
        public void EXPAND0008()
        {
            DiagnosticDescriptors.EXPAND0008_EmbeddedDataError("Anotherlib", "SourceExpander.EmbeddedSourceCode", "There was an error deserializing the object of type SourceExpander.SourceFileInfo[]. Encountered unexpected character '}'.")
                .GetMessage(FormatProvider)
                .Should()
                .Be(FormatProvider.Name switch
                {
                    _ => "Invalid embedded data: Anotherlib, Key: SourceExpander.EmbeddedSourceCode, Message: There was an error deserializing the object of type SourceExpander.SourceFileInfo[]. Encountered unexpected character '}'.",
                });
        }
        [Fact]
        public void EXPAND0009()
        {
            DiagnosticDescriptors.EXPAND0009_MetadataEmbeddingFileNotFound("Program.cs")
                .GetMessage(FormatProvider)
                .Should()
                .Be(FormatProvider.Name switch
                {
                    _ => "MetadataEmbeddingFile is not found: name: Program.cs",
                });
        }
    }
}
