using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using SourceExpander.Roslyn;

namespace SourceExpander
{
    [Generator]
    public class DummyGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context) { }
        public void Execute(GeneratorExecutionContext context)
        {
            var parseOptions = (CSharpParseOptions)context.ParseOptions;
            parseOptions = parseOptions.WithLanguageVersion(LanguageVersion.CSharp4);
            var compilation = (CSharpCompilation)context.Compilation;
            var rewriter = new DummyRewriter();
            var list = new List<SyntaxTree>(compilation.SyntaxTrees.Length);
            foreach (var tree in compilation.SyntaxTrees)
            {
                var newRoot = rewriter.Visit(tree.GetRoot(context.CancellationToken));
                list.Add(tree.WithRootAndOptions(newRoot, parseOptions));
            }
            compilation = compilation.RemoveAllSyntaxTrees().AddSyntaxTrees(list);


            var resolver = new EmbeddingResolver(
                compilation,
                parseOptions,
                new DummyDiagnosticReporter(),
                new EmbedderConfig(
                    true,
                    EmbeddingType.GZipBase32768,
                    excludeAttributes: new[] {
                        "System.Runtime.CompilerServices.MethodImplAttribute",
                        "System.Runtime.CompilerServices.CallerFilePathAttribute"
                    },
                    enableMinify: true),
                context.CancellationToken);

            context.AddSource(
                "EmbeddedSourceCode.Metadata.Generated.cs", CreateMetadataSource(resolver.EnumerateAssemblyMetadata()));
        }

        private static SourceText CreateMetadataSource(IEnumerable<(string Key, string Value)> metadatas)
        {
            var sb = new StringBuilder("using System.Reflection;");
            foreach (var (Key, Value) in metadatas)
            {
                sb.Append("[assembly: AssemblyMetadataAttribute(");
                sb.Append(Key.ToLiteral());
                sb.Append(",");
                sb.Append(Value.ToLiteral());
                sb.AppendLine(")]");
            }
            return SourceText.From(sb.ToString(), Encoding.UTF8);
        }
    }

    public class DummyDiagnosticReporter : IDiagnosticReporter
    {
        public DummyDiagnosticReporter() { }
        public void ReportDiagnostic(Diagnostic diagnostic) { }
    }
}
