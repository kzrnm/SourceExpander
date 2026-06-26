using System.Globalization;
using Microsoft.CodeAnalysis.CSharp;

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
    public async Task EXPAND0001()
    {
        await DiagnosticDescriptors.EXPAND0001_UnknownError("LX")
            .GetMessage(FormatProvider)
            .Should().BeEqualTo(FormatProvider.Name switch
            {
                "ja-JP" => "不明なエラー: LX",
                _ => "Unknown error: LX",
            });
    }
    [Test]
    public async Task EXPAND0002()
    {
        await DiagnosticDescriptors.EXPAND0002_ExpanderVersion(new Version(2, 0, 0), "Newerlib", new Version(3, 0, 0))
            .GetMessage(FormatProvider)
            .Should().BeEqualTo(FormatProvider.Name switch
            {
                "ja-JP" => "Expander version(2.0.0) が Newerlib(3.0.0) の embedder より古いです",
                _ => "Expander version(2.0.0) is older than embedder of Newerlib(3.0.0)",
            });
    }
    [Test]
    public async Task EXPAND0003()
    {
        await DiagnosticDescriptors.EXPAND0003_NotFoundEmbedded()
            .GetMessage(FormatProvider)
            .Should().BeEqualTo(FormatProvider.Name switch
            {
                "ja-JP" => "埋め込みソースが見つかりません",
                _ => "Not found embedded source",
            });
    }
    [Test]
    public async Task EXPAND0004()
    {
        await DiagnosticDescriptors.EXPAND0004_MustBeNewerThanCSharp3()
            .GetMessage(FormatProvider)
            .Should().BeEqualTo(FormatProvider.Name switch
            {
                _ => "Need C# 3 or later",
            });
    }
    [Test]
    public async Task EXPAND0005()
    {
        await DiagnosticDescriptors.EXPAND0005_NewerCSharpVersion(LanguageVersion.CSharp7, "Newerlib", LanguageVersion.CSharp8)
            .GetMessage(FormatProvider)
            .Should().BeEqualTo(FormatProvider.Name switch
            {
                "ja-JP" => "C# のバージョン(7.0) が埋め込まれている Newerlib(8.0) より古いです。",
                _ => "C# version(7.0) is older than embedded Newerlib(8.0)",
            });
    }
    [Test]
    public async Task EXPAND0007()
    {
        await DiagnosticDescriptors.EXPAND0007_ParseConfigError("/home/source/SourceExpander.Generator.Config.json", "any error")
            .GetMessage(FormatProvider)
            .Should().BeEqualTo(FormatProvider.Name switch
            {
                _ => "Error config file: Path: /home/source/SourceExpander.Generator.Config.json, Message: any error",
            });
    }
    [Test]
    public async Task EXPAND0008()
    {
        await DiagnosticDescriptors.EXPAND0008_EmbeddedDataError("Anotherlib", "SourceExpander.EmbeddedSourceCode", "There was an error deserializing the object of type SourceExpander.SourceFileInfo[]. Encountered unexpected character '}'.")
            .GetMessage(FormatProvider)
            .Should().BeEqualTo(FormatProvider.Name switch
            {
                _ => "Invalid embedded data: Anotherlib, Key: SourceExpander.EmbeddedSourceCode, Message: There was an error deserializing the object of type SourceExpander.SourceFileInfo[]. Encountered unexpected character '}'.",
            });
    }
    [Test]
    public async Task EXPAND0009()
    {
        await DiagnosticDescriptors.EXPAND0009_MetadataEmbeddingFileNotFound("Program.cs")
            .GetMessage(FormatProvider)
            .Should().BeEqualTo(FormatProvider.Name switch
            {
                _ => "MetadataEmbeddingFile is not found: name: Program.cs",
            });
    }
    [Test]
    public async Task EXPAND0010()
    {
        await DiagnosticDescriptors.EXPAND0010_UnsafeBlock("/home/mine/P.cs")
            .GetMessage(FormatProvider)
            .Should().BeEqualTo(FormatProvider.Name switch
            {
                _ => "Expanded code has unsafe block: /home/mine/P.cs",
            });
    }
}
