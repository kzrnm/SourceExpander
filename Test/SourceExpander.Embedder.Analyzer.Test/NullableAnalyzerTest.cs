using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using Xunit;
using VerifyCS = SourceExpander.Embedder.Analyzer.Test.CSharpAnalyzerVerifier<SourceExpander.NullableAnalyzer>;

namespace SourceExpander.Embedder.Analyzer.Test
{
    public class NullableAnalyzerTest
    {
        [Theory]
        [InlineData(NullableContextOptions.Disable)]
        [InlineData(NullableContextOptions.Warnings)]
        public async Task NotDiagnostic(NullableContextOptions nullable)
        {
            var source = @"
using System;
using System.Collections.Generic;

namespace Foo
{
    using static System.Console;
    using MM = System.Math;

    public interface IAny<T> {
        T Prop1 { set; get; }
        T Prop2 { get; set; }
    }
}
";
            var test = new NullableCompilationTest(nullable)
            {
                TestCode = source,
            };
            await test.RunAsync(CancellationToken.None);
        }

        [Theory]
        [InlineData(NullableContextOptions.Annotations)]
        [InlineData(NullableContextOptions.Enable)]
        public async Task NullableCompilation(NullableContextOptions nullable)
        {
            var source = @"
using System;
using System.Collections.Generic;

namespace Foo
{
    using static System.Console;
    using MM = System.Math;

    public interface IAny<T> {
        T Prop1 { set; get; }
        T Prop2 { get; set; }
    }
}
";
            var expected = new DiagnosticResult[]
            {
                VerifyCS.Diagnostic("EMBEDDER0003")
            };
            var test = new NullableCompilationTest(nullable)
            {
                TestCode = source,
            };
            test.ExpectedDiagnostics.AddRange(expected);
            await test.RunAsync(CancellationToken.None);
        }

        [Fact]
        public async Task NullableDirective()
        {
            var source = @"
using System;
using System.Collections.Generic;
#nullable enable
namespace Foo
{
    using static System.Console;
    using MM = System.Math;

    public interface IAny<T> {
        T Prop1 { set; get; }
        T Prop2 { get; set; }
    }
}
#nullable restore
";
            var expected = new DiagnosticResult[]
            {
                VerifyCS.Diagnostic("EMBEDDER0004").WithSpan(4, 1, 4, 17),
                VerifyCS.Diagnostic("EMBEDDER0004").WithSpan(15, 1, 15, 18),
            };
            var test = new NullableCompilationTest(NullableContextOptions.Disable)
            {
                TestCode = source,
            };
            test.ExpectedDiagnostics.AddRange(expected);
            await test.RunAsync(CancellationToken.None);
        }

        public class NullableCompilationTest : CSharpAnalyzerTest<NullableAnalyzer, XUnitVerifier>
        {
            public NullableCompilationTest(NullableContextOptions nullable)
            {
                SolutionTransforms.Add((solution, projectId) =>
                {
                    var compilationOptions = (CSharpCompilationOptions)solution.GetProject(projectId).CompilationOptions;
                    compilationOptions = compilationOptions
                    .WithNullableContextOptions(nullable)
                    .WithSpecificDiagnosticOptions(
                        compilationOptions.SpecificDiagnosticOptions.SetItems(CSharpVerifierHelper.NullableWarnings));
                    solution = solution.WithProjectCompilationOptions(projectId, compilationOptions);

                    return solution;
                });
            }
        }
    }
}
