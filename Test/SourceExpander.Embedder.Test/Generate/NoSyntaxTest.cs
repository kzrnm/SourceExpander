using System.Threading.Tasks;

namespace SourceExpander.Generate
{
    public class NoSyntaxTest : EmbedderGeneratorTestBase
    {
        [Fact]
        public async Task Generate()
        {
            var test = new Test
            {
                TestState =
                {
                    AdditionalFiles =
                    {
                        enableMinifyJson,
                    },
                }
            };
            await test.RunAsync();
        }
    }
}
