using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace SourceExpander.Embedder.Generate.Test
{
    public class CompilationErrorTest : EmbeddingGeneratorTestBase
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
                    Sources = {
                        (
                            "home/test/Program.cs",
                            @"
class Program
{
    public static int Method() => 1 + 2 ** 3;
}
"
                        ),
                    },
                    ExpectedDiagnostics =
                    {
                        DiagnosticResult.CompilerError("CS0193").WithSpan("home/test/Program.cs", 4, 42, 4, 45),
                    },
                }
            };
            await test.RunAsync();
        }
    }
}
