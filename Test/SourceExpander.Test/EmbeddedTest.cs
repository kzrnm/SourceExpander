using Microsoft.CodeAnalysis.CSharp;

namespace SourceExpander.Test;

public class EmbeddedTest
{
    [Test]
    public async Task Embedded()
    {
        var embedded = await EmbeddedData.LoadFromAssembly(typeof(Expander));
        await embedded.EmbeddedLanguageVersion.Should().BeEqualTo(LanguageVersion.CSharp4.ToDisplayString());
        var s = await Assert.That(embedded.SourceFiles).HasSingleItem();
        using (Assert.Multiple())
        {
            await s.FileName.Should().BeEqualTo("SourceExpander>Expander.cs");
            await s.TypeNames.Should().BeEquivalentTo(["SourceExpander.Expander"]);
            await s.Usings.Should().BeEquivalentTo(["using System.Diagnostics;"]);
            await s.Dependencies.Should().BeEmpty();
            await s.CodeBody.Should().BeEqualTo(
                "namespace SourceExpander{public class Expander{[Conditional(\"EXP\")]" +
                "public static void Expand(string inputFilePath=null,string outputFilePath=null,bool ignoreAnyError=true){}" +
                "public static string ExpandString(string inputFilePath=null,bool ignoreAnyError=true){return \"\";}}}");
        }
    }
}
