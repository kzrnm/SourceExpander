using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using Xunit;
using VerifyCS = SourceExpander.Embedder.Analyzer.Test.CSharpAnalyzerVerifier<SourceExpander.ExpandEmbeddedAnalyzer>;

namespace SourceExpander.Embedder.Analyzer.Test
{
    public class ExpandEmbeddedAnalyzerTest
    {
        public static readonly TheoryData Expand_Data = new TheoryData<string, DiagnosticResult[]>
        {
            {
                @"
using System;
class Program
{
    static void Main()
    {
        SourceExpander.Expander.Expand();
        Console.WriteLine(1);
    }
}
",
                new DiagnosticResult[]
                {
                    VerifyCS.Diagnostic().WithSpan(7, 9, 7, 41),
                }
            },
            {
                @"
using System;
using SourceExpander;
class Program
{
    static void Main()
    {
        Expander
            .Expand();
        Console.WriteLine(1);
    }
}
",
                new DiagnosticResult[]
                {
                    VerifyCS.Diagnostic().WithSpan(8, 9, 9, 22),
                }
            }
        };

        [Theory]
        [MemberData(nameof(Expand_Data))]
        public async Task Expand(string source, DiagnosticResult[] expected)
        {
            var test = new ExpandCompilationTest
            {
                TestCode = source,
            };
            test.ExpectedDiagnostics.AddRange(expected);
            await test.RunAsync(CancellationToken.None);
        }

        public class ExpandCompilationTest : CSharpAnalyzerTest<ExpandEmbeddedAnalyzer, XUnitVerifier>
        {
            public ExpandCompilationTest()
            {
                ReferenceAssemblies = ReferenceAssemblies.WithPackages(
                    ImmutableArray.Create(new PackageIdentity("SourceExpander", "2.6.0")));
                SolutionTransforms.Add((solution, projectId) =>
                {
                    var compilationOptions = solution.GetProject(projectId).CompilationOptions;
                    compilationOptions = compilationOptions.WithSpecificDiagnosticOptions(
                        compilationOptions.SpecificDiagnosticOptions.SetItems(CSharpVerifierHelper.NullableWarnings));
                    solution = solution.WithProjectCompilationOptions(projectId, compilationOptions);

                    return solution;
                });
            }
        }
    }
}
