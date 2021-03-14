#nullable disable
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;

namespace SourceExpander
{

    public class CSharpSourceGeneratorTest<TSourceGenerator> : CSharpSourceGeneratorTest<TSourceGenerator, XUnitVerifier>
        where TSourceGenerator : ISourceGenerator, new()
    {
        public CSharpCompilationOptions CompilationOptions { get; set; }
        protected override CompilationOptions CreateCompilationOptions() => CompilationOptions ?? base.CreateCompilationOptions();
        public CSharpParseOptions ParseOptions { get; set; }
        protected override ParseOptions CreateParseOptions() => ParseOptions ?? base.CreateParseOptions();
    }
}
