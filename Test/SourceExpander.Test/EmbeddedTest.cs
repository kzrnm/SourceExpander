using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;

namespace SourceExpander.Test
{
    public class EmbeddedTest
    {
        [Fact]
        public async Task Embedded()
        {
            var embedded = await EmbeddedData.LoadFromAssembly(typeof(Expander));
            embedded.EmbeddedLanguageVersion.ShouldBe(LanguageVersion.CSharp4.ToDisplayString());
            embedded.SourceFiles.ShouldHaveSingleItem()
                .ShouldBeEquivalentTo(new SourceFileInfo(
                    "SourceExpander>Expander.cs",
                    ["SourceExpander.Expander"],
                    ["using System.Diagnostics;"],
                    [],
                    "namespace SourceExpander{public class Expander{[Conditional(\"EXP\")]" +
                    "public static void Expand(string inputFilePath=null,string outputFilePath=null,bool ignoreAnyError=true){}" +
                    "public static string ExpandString(string inputFilePath=null,bool ignoreAnyError=true){return \"\";}}}"));
        }
    }
}
