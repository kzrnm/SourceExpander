using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;
using SourceExpander.Roslyn;

namespace SourceExpander
{
    [Generator]
    public class EmbedderGenerator : ISourceGenerator
    {
        private const string CONFIG_FILE_NAME = "SourceExpander.Embedder.Config.json";
        public void Initialize(GeneratorInitializationContext context) { }
        public void Execute(GeneratorExecutionContext context)
        {
            var configText = context.AdditionalFiles
                .FirstOrDefault(a =>
                    StringComparer.OrdinalIgnoreCase.Compare(Path.GetFileName(a.Path), CONFIG_FILE_NAME) == 0)
                ?.GetText(context.CancellationToken);

            EmbedderConfig config;
            try
            {
                config = EmbedderConfig.Parse(configText, context.CancellationToken);
            }
            catch (ParseConfigException e)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.EMBED0003_ParseConfigError, Location.None, e.Message));
                return;
            }
            if (!config.Enabled)
                return;

            if (!ValidateCompilation(context.Compilation))
                return;

            var embeddingContext = new EmbeddingContext(
                (CSharpCompilation)context.Compilation,
                (CSharpParseOptions)context.ParseOptions,
                new DiagnosticReporter(context),
                config,
                context.CancellationToken);

            var resolver = new EmbeddingResolver(embeddingContext);
            var resolvedSources = resolver.ResolveFiles();

            if (resolvedSources.Length == 0)
                return;

            context.AddSource(
                "EmbeddedSourceCode.Metadata.Generated.cs", CreateMetadataSource(resolver.EnumerateAssemblyMetadata()));
        }

        private static SourceText CreateMetadataSource(ImmutableDictionary<string, string> metadatas)
        {
            var sb = new StringBuilder("using System.Reflection;");
            foreach (var p in metadatas)
            {
                sb.Append("[assembly: AssemblyMetadataAttribute(");
                sb.Append(p.Key.ToLiteral());
                sb.Append(",");
                sb.Append(p.Value.ToLiteral());
                sb.AppendLine(")]");
            }
            return SourceText.From(sb.ToString(), Encoding.UTF8);
        }

        private static bool ValidateCompilation(Compilation compilation)
        {
            using var ms = new MemoryStream();
            return ValidateCompilation(compilation.Emit(ms));
        }
        private static bool ValidateCompilation(EmitResult result)
            => result.Success
            && !result.Diagnostics.Any(d => d.IsWarningAsError || d.Severity == DiagnosticSeverity.Error);

        private bool ValidateEmbeddedSources(EmbeddingContext context, ImmutableArray<SourceFileInfo> sources)
        {
            SyntaxTree ToSyntaxTree(SourceFileInfo source)
                => CSharpSyntaxTree.ParseText(
                    source.Restore(),
                    context.ParseOptions,
                    cancellationToken: context.CancellationToken);

            var embeddedCompilation = CSharpCompilation.Create("NewCompilation",
                sources.Select(s => ToSyntaxTree(s)),
                context.Compilation.References,
                context.Compilation.Options);
            return ValidateCompilation(embeddedCompilation);
        }
    }
}
