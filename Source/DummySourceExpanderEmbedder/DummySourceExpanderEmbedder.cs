using System;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using SourceExpander.Roslyn;

namespace SourceExpander
{
    [Generator]
    public class DummySourceExpanderEmbedder : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context) { }
        public void Execute(GeneratorExecutionContext context)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream("DummySourceExpanderEmbedder.Expander.cs");

            var opts = ((CSharpParseOptions)context.ParseOptions).WithLanguageVersion(LanguageVersion.CSharp4);
            var compilation = CSharpCompilation.Create(
                assemblyName: "DummySourceExpander",
                syntaxTrees: new[] {
                    CSharpSyntaxTree.ParseText(
                        SourceText.From(stream, Encoding.UTF8))
                },
                references: context.Compilation.References,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            if (!compilation.GetDiagnostics(context.CancellationToken).IsEmpty)
                throw new InvalidProgramException();

            var resolver = new EmbeddingResolver(
                compilation,
                opts,
                new DiagnosticReporter(context),
                context.CancellationToken);

            foreach (var (path, source) in resolver.EnumerateEmbeddingSources())
            {
                context.AddSource(path, source);
            }
        }
    }
}
