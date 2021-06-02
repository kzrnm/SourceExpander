using System;
using System.Globalization;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Xunit;

namespace SourceExpander.Diagnostics
{
    public class DiagnosticDescriptorsTestJaJP
    {
        static readonly IFormatProvider formatProvider = new CultureInfo("ja-JP");
        [Fact]
        public void EMBED0001()
        {
            DiagnosticDescriptors.EMBED0001_UnknownError("LX")
                .GetMessage(formatProvider)
                .Should()
                .Be("不明なエラー: LX");
        }
        [Fact]
        public void EMBED0002()
        {
            DiagnosticDescriptors.EMBED0002_OlderVersion(new Version(2, 0, 0), "Newerlib", new Version(3, 0, 0))
                .GetMessage(formatProvider)
                .Should()
                .Be("Embeder version(2.0.0) が Newerlib(3.0.0) の Embedder より古いです");
        }
        [Fact]
        public void EMBED0003()
        {
            DiagnosticDescriptors.EMBED0003_ParseConfigError("/home/source/SourceExpander.Embedder.Config.json", "any error")
                .GetMessage(formatProvider)
                .Should()
                .Be("Error config file: /home/source/SourceExpander.Embedder.Config.json");
        }
        [Fact]
        public void EMBED0004()
        {
            DiagnosticDescriptors.EMBED0004_ErrorEmbeddedSource("P.cs", "any error")
                .GetMessage(formatProvider)
                .Should()
                .Be("Error embedded source: File: P.cs, Message: any error");
        }
        [Fact]
        public void EMBED0005()
        {
            DiagnosticDescriptors.EMBED0005_EmbeddedSourceDiff("Uns")
                .GetMessage(formatProvider)
                .Should()
                .Be("Different syntax: near Uns. This is Embedder error, please report this to GitHub repository.");
        }
        [Fact]
        public void EMBED0006()
        {
            DiagnosticDescriptors.EMBED0006_AnotherAssemblyEmbeddedDataError("Other", "SourceExpander.EmbeddedSourceCode", "There was an error deserializing the object of type SourceExpander.SourceFileInfo[]. Encountered unexpected character '}'.")
                .GetMessage(formatProvider)
                .Should()
                .Be("他のアセンブリの埋め込みデータが不正です: Other, Key: SourceExpander.EmbeddedSourceCode, Message: There was an error deserializing the object of type SourceExpander.SourceFileInfo[]. Encountered unexpected character '}'.");
        }
        [Fact]
        public void EMBED0007()
        {
            DiagnosticDescriptors.EMBED0007_NullableProject()
                .GetMessage(formatProvider)
                .Should()
                .Be("nullableなプロジェクトは未対応です");
        }
        [Fact]
        public void EMBED0008()
        {
            DiagnosticDescriptors.EMBED0008_NullableDirective(Location.None)
                .GetMessage(formatProvider)
                .Should()
                .Be("nullableディレクティブは未対応です");
        }
        [Fact]
        public void EMBED0009()
        {
            DiagnosticDescriptors.EMBED0009_UsingStaticDirective(Location.None)
                .GetMessage(formatProvider)
                .Should()
                .Be("名前衝突の危険があるため using static ディレクティブは非推奨です");
        }
        [Fact]
        public void EMBED0010()
        {
            DiagnosticDescriptors.EMBED0010_UsingAliasDirective(Location.None)
                .GetMessage(formatProvider)
                .Should()
                .Be("名前衝突の危険があるため using alias ディレクティブは非推奨です");
        }
        [Fact]
        public void EMBED0011()
        {
            DiagnosticDescriptors.EMBED0011_ObsoleteConfigProperty(
                Location.None, "/home/user/SourceExpander.Embedder.Config.json", "old-property", "instead-property")
                .GetMessage(formatProvider)
                .Should()
                .Be("/home/user/SourceExpander.Embedder.Config.json: old-property は廃止されました。代わりに instead-property を使用してください");
        }
    }
}
