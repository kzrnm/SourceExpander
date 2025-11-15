using System.Threading.Tasks;

namespace SourceExpander.Generate
{
    public class NoSyntaxTest : EmbedderGeneratorTestBase
    {
        [Test]
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
            await test.RunAsync(TestContext.Current!.Execution.CancellationToken);
        }
    }
}
