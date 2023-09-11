using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using SourceExpander.Roslyn;

namespace SourceExpander
{
    [Generator]
    public class DummyGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterImplementationSourceOutput(context.CompilationProvider.Combine(context.ParseOptionsProvider), Execute);
        }

        private void Execute(SourceProductionContext ctx, (Compilation Left, ParseOptions Right) source)
        {
            var (compilationOrig, parseOptionsOrig) = source;
            var parseOptions = (CSharpParseOptions)parseOptionsOrig;
            parseOptions = parseOptions.WithLanguageVersion(LanguageVersion.CSharp4);
            var compilation = (CSharpCompilation)compilationOrig;
            var rewriter = new DummyRewriter();
            var list = new List<SyntaxTree>(compilation.SyntaxTrees.Length);
            foreach (var tree in compilation.SyntaxTrees)
            {
                if (tree.FilePath.EndsWith("Resources.Designer.cs"))
                    continue;
                var newRoot = rewriter.Visit(tree.GetRoot(ctx.CancellationToken));
                list.Add(tree.WithRootAndOptions(newRoot, parseOptions));
            }
            compilation = compilation.RemoveAllSyntaxTrees().AddSyntaxTrees(list);

            var resolver = new EmbeddingResolver(
                compilation,
                parseOptions,
                new DummyDiagnosticReporter(),
                new EmbedderConfig()
                {
                    EmbeddingType = EmbeddingType.GZipBase32768,
                    ExcludeAttributes = ImmutableHashSet.CreateRange(new[]
                    {
                        "System.Runtime.CompilerServices.MethodImplAttribute",
                        "System.Runtime.CompilerServices.CallerFilePathAttribute"
                    }),
                    MinifyLevel = MinifyLevel.Full,
                },
                ctx.CancellationToken);

            ctx.AddSource(
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
