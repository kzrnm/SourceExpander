using System.Collections.Immutable;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace SourceExpander.Test
{
    public class EmbeddedTest
    {
        [Fact]
        public async Task Embedded()
        {
            var embedded = await EmbeddedData.LoadFromAssembly(typeof(Expander));
            embedded.EmbeddedLanguageVersion.Should().Be(LanguageVersion.CSharp4.ToDisplayString());
            embedded.SourceFiles.Should().ContainSingle()
                .Which
                .Should()
                .BeEquivalentTo(new SourceFileInfo(
                    "SourceExpander>Expander.cs",
                    ImmutableArray.Create("SourceExpander.Expander"),
                    ImmutableArray.Create("using System.Diagnostics;"),
                    ImmutableArray<string>.Empty,
                    "namespace SourceExpander{public class Expander{[Conditional(\"EXP\")]" +
                    "public static void Expand(string inputFilePath=null,string outputFilePath=null,bool ignoreAnyError=true){}" +
                    "public static string ExpandString(string inputFilePath=null,bool ignoreAnyError=true){return \"\";}}}"));
        }
    }
}
