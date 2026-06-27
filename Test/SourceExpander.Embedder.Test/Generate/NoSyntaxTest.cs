namespace SourceExpander.Generate;

public class NoSyntaxTest : EmbedderGeneratorTestBase
{
    [Test]
    public async Task Generate(CancellationToken cancellationToken)
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
        await test.RunAsync(cancellationToken);
    }
}
