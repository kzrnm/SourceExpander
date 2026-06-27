using System.Globalization;
using Microsoft.CodeAnalysis;

namespace SourceExpander.Diagnostics;

[InheritsTests]
public class DiagnosticDescriptorsTest : DiagnosticDescriptorsTestBase
{
    protected override CultureInfo FormatProvider => CultureInfo.InvariantCulture;
}

[InheritsTests]
public class DiagnosticDescriptorsTestJaJp : DiagnosticDescriptorsTestBase
{
    protected override CultureInfo FormatProvider => new("ja-JP");
}
public abstract class DiagnosticDescriptorsTestBase
{
    protected abstract CultureInfo FormatProvider { get; }
    [Test]
    public async Task EMBED0001()
    {
        await DiagnosticDescriptors.EMBED0001_UnknownError("LX")
            .GetMessage(FormatProvider)
            .Should().BeEqualTo(FormatProvider.Name switch
            {
                "ja-JP" => "不明なエラー: LX",
                _ => "Unknown error: LX",
            });
    }
    [Test]
    public async Task EMBED0002()
    {
        await DiagnosticDescriptors.EMBED0002_OlderVersion(new Version(2, 0, 0), "Newerlib", new Version(3, 0, 0))
            .GetMessage(FormatProvider)
            .Should().BeEqualTo(FormatProvider.Name switch
            {
                "ja-JP" => "Embeder version(2.0.0) が Newerlib(3.0.0) の Embedder より古いです",
                _ => "Embeder version(2.0.0) is older than embedder of Newerlib(3.0.0)",
            });
    }
    [Test]
    public async Task EMBED0003()
    {
        await DiagnosticDescriptors.EMBED0003_ParseConfigError("/home/source/SourceExpander.Embedder.Config.json", "any error")
            .GetMessage(FormatProvider)
            .Should().BeEqualTo(FormatProvider.Name switch
            {
                _ => "Error config file: /home/source/SourceExpander.Embedder.Config.json",
            });
    }
    [Test]
    public async Task EMBED0004()
    {
        await DiagnosticDescriptors.EMBED0004_ErrorEmbeddedSource("P.cs", "any error")
            .GetMessage(FormatProvider)
            .Should().BeEqualTo(FormatProvider.Name switch
            {
                _ => "Error embedded source: File: P.cs, Message: any error",
            });
    }
    [Test]
    public async Task EMBED0005()
    {
        await DiagnosticDescriptors.EMBED0005_EmbeddedSourceDiff("Uns")
            .GetMessage(FormatProvider)
            .Should().BeEqualTo(FormatProvider.Name switch
            {
                _ => "Different syntax: near Uns. This is Embedder error, please report this to GitHub repository.",
            });
    }
    [Test]
    public async Task EMBED0006()
    {
        await DiagnosticDescriptors.EMBED0006_AnotherAssemblyEmbeddedDataError("Other", "SourceExpander.EmbeddedSourceCode", "There was an error deserializing the object of type SourceExpander.SourceFileInfo[]. Encountered unexpected character '}'.")
            .GetMessage(FormatProvider)
            .Should().BeEqualTo(FormatProvider.Name switch
            {
                "ja-JP" => "他のアセンブリの埋め込みデータが不正です: Other, Key: SourceExpander.EmbeddedSourceCode, Message: There was an error deserializing the object of type SourceExpander.SourceFileInfo[]. Encountered unexpected character '}'.",
                _ => "Another assembly has invalid embedded data: Other, Key: SourceExpander.EmbeddedSourceCode, Message: There was an error deserializing the object of type SourceExpander.SourceFileInfo[]. Encountered unexpected character '}'.",
            });
    }
    [Test]
    public async Task EMBED0009()
    {
        await DiagnosticDescriptors.EMBED0009_UsingStaticDirective(Location.None)
            .GetMessage(FormatProvider)
            .Should().BeEqualTo(FormatProvider.Name switch
            {
                "ja-JP" => "名前衝突の危険があるため using static ディレクティブは非推奨です",
                _ => "Avoid using static directive because there is a risk of name collision",
            });
    }
    [Test]
    public async Task EMBED0010()
    {
        await DiagnosticDescriptors.EMBED0010_UsingAliasDirective(Location.None)
            .GetMessage(FormatProvider)
            .Should().BeEqualTo(FormatProvider.Name switch
            {
                "ja-JP" => "名前衝突の危険があるため using alias ディレクティブは非推奨です",
                _ => "Avoid using alias directive because there is a risk of name collision",
            });
    }
    [Test]
    public async Task EMBED0011()
    {
        await DiagnosticDescriptors.EMBED0011_ObsoleteConfigProperty(
            "/home/user/SourceExpander.Embedder.Config.json", "old-property", "instead-property")
            .GetMessage(FormatProvider)
            .Should().BeEqualTo(FormatProvider.Name switch
            {
                "ja-JP" => "/home/user/SourceExpander.Embedder.Config.json: old-property は廃止されました。代わりに instead-property を使用してください",
                _ => "/home/user/SourceExpander.Embedder.Config.json: Obsolete embedder config property. old-property is obsolete. Use instead-property.",
            });
    }
}
