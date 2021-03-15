using System.Threading.Tasks;
using Xunit;

namespace SourceExpander.Embedder.Generate.Test
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
