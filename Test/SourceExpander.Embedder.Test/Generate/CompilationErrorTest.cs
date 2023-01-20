using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace SourceExpander.Generate
{
    public class CompilationErrorTest : EmbedderGeneratorTestBase
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
                            """
class Program
{
    public static int Method() => 1 + 2 ** 3;
}
"""
                        ),
                    },
                    ExpectedDiagnostics =
                    {
                        DiagnosticResult.CompilerError("CS0193").WithSpan("home/test/Program.cs", 3, 42, 3, 45),
                    },
                }
            };
            await test.RunAsync();
        }
    }
}
