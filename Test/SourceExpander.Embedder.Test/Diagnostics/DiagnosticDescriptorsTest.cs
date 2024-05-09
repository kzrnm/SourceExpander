using System;
using System.Globalization;
using FluentAssertions;
using Microsoft.CodeAnalysis;
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
        public void EMBED0001()
        {
            DiagnosticDescriptors.EMBED0001_UnknownError("LX")
                .GetMessage(FormatProvider)
                .Should()
                .Be(FormatProvider.Name switch
                {
                    "ja-JP" => "不明なエラー: LX",
                    _ => "Unknown error: LX",
                });
        }
        [Fact]
        public void EMBED0002()
        {
            DiagnosticDescriptors.EMBED0002_OlderVersion(new Version(2, 0, 0), "Newerlib", new Version(3, 0, 0))
                .GetMessage(FormatProvider)
                .Should()
                .Be(FormatProvider.Name switch
                {
                    "ja-JP" => "Embeder version(2.0.0) が Newerlib(3.0.0) の Embedder より古いです",
                    _ => "Embeder version(2.0.0) is older than embedder of Newerlib(3.0.0)",
                });
        }
        [Fact]
        public void EMBED0003()
        {
            DiagnosticDescriptors.EMBED0003_ParseConfigError("/home/source/SourceExpander.Embedder.Config.json", "any error")
                .GetMessage(FormatProvider)
                .Should()
                .Be(FormatProvider.Name switch
                {
                    _ => "Error config file: /home/source/SourceExpander.Embedder.Config.json",
                });
        }
        [Fact]
        public void EMBED0004()
        {
            DiagnosticDescriptors.EMBED0004_ErrorEmbeddedSource("P.cs", "any error")
                .GetMessage(FormatProvider)
                .Should()
                .Be(FormatProvider.Name switch
                {
                    _ => "Error embedded source: File: P.cs, Message: any error",
                });
        }
        [Fact]
        public void EMBED0005()
        {
            DiagnosticDescriptors.EMBED0005_EmbeddedSourceDiff("Uns")
                .GetMessage(FormatProvider)
                .Should()
                .Be(FormatProvider.Name switch
                {
                    _ => "Different syntax: near Uns. This is Embedder error, please report this to GitHub repository.",
                });
        }
        [Fact]
        public void EMBED0006()
        {
            DiagnosticDescriptors.EMBED0006_AnotherAssemblyEmbeddedDataError("Other", "SourceExpander.EmbeddedSourceCode", "There was an error deserializing the object of type SourceExpander.SourceFileInfo[]. Encountered unexpected character '}'.")
                .GetMessage(FormatProvider)
                .Should()
                .Be(FormatProvider.Name switch
                {
                    "ja-JP" => "他のアセンブリの埋め込みデータが不正です: Other, Key: SourceExpander.EmbeddedSourceCode, Message: There was an error deserializing the object of type SourceExpander.SourceFileInfo[]. Encountered unexpected character '}'.",
                    _ => "Another assembly has invalid embedded data: Other, Key: SourceExpander.EmbeddedSourceCode, Message: There was an error deserializing the object of type SourceExpander.SourceFileInfo[]. Encountered unexpected character '}'.",
                });
        }
        [Fact]
        public void EMBED0009()
        {
            DiagnosticDescriptors.EMBED0009_UsingStaticDirective(Location.None)
                .GetMessage(FormatProvider)
                .Should()
                .Be(FormatProvider.Name switch
                {
                    "ja-JP" => "名前衝突の危険があるため using static ディレクティブは非推奨です",
                    _ => "Avoid using static directive because there is a risk of name collision",
                });
        }
        [Fact]
        public void EMBED0010()
        {
            DiagnosticDescriptors.EMBED0010_UsingAliasDirective(Location.None)
                .GetMessage(FormatProvider)
                .Should()
                .Be(FormatProvider.Name switch
                {
                    "ja-JP" => "名前衝突の危険があるため using alias ディレクティブは非推奨です",
                    _ => "Avoid using alias directive because there is a risk of name collision",
                });
        }
        [Fact]
        public void EMBED0011()
        {
            DiagnosticDescriptors.EMBED0011_ObsoleteConfigProperty(
                "/home/user/SourceExpander.Embedder.Config.json", "old-property", "instead-property")
                .GetMessage(FormatProvider)
                .Should()
                .Be(FormatProvider.Name switch
                {
                    "ja-JP" => "/home/user/SourceExpander.Embedder.Config.json: old-property は廃止されました。代わりに instead-property を使用してください",
                    _ => "/home/user/SourceExpander.Embedder.Config.json: Obsolete embedder config property. old-property is obsolete. Use instead-property.",
                });
        }
    }
}
