using System.Threading.Tasks;
using Xunit;
using VerifyCS = AtCoderAnalyzer.Test.CSharpAnalyzerVerifier<SourceExpander.UsingDirectiveAnalyzer>;

namespace AtCoderAnalyzer.Test
{
    public class UsingDirectiveAnalyzerTest
    {
        [Fact]
        public async Task NotDiagnostic()
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
            await VerifyCS.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task Diagnostic()
        {
            var source = @"
using System;
using static System.Console;
using MM = System.Math;

public interface IAny<T> {
    T Prop1 { set; get; }
    T Prop2 { get; set; }
}
";
            await VerifyCS.VerifyAnalyzerAsync(source,
                VerifyCS.Diagnostic("EMBEDDER0001").WithSpan(3, 1, 3, 29),
                VerifyCS.Diagnostic("EMBEDDER0002").WithSpan(4, 1, 4, 24));
        }
    }
}
