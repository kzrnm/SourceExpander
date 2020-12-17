using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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
#if DEBUG
            if (!System.Diagnostics.Debugger.IsAttached)
            {
                //System.Diagnostics.Debugger.Launch();
            }
#endif

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

            if (context.Compilation.GetDiagnostics(context.CancellationToken).HasCompilationError())
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

            var sb = CreateMetadataSource(new StringBuilder(), resolver.EnumerateAssemblyMetadata());

            if (config.EmbeddingSourceClass.Enabled)
                CreateEmbbedingSourceClass(sb, resolvedSources, config.EmbeddingSourceClass.IfDirective);

            context.AddSource(
                "EmbeddedSourceCode.Metadata.Generated.cs",
                SourceText.From(sb.ToString(), Encoding.UTF8));
        }

        private static StringBuilder CreateMetadataSource(StringBuilder sb, ImmutableDictionary<string, string> metadatas)
        {
            sb.AppendLine("using System.Reflection;");
            foreach (var p in metadatas)
            {
                sb.Append("[assembly: AssemblyMetadataAttribute(");
                sb.Append(p.Key.ToLiteral());
                sb.Append(",");
                sb.Append(p.Value.ToLiteral());
                sb.AppendLine(")]");
            }
            return sb;
        }

        private static StringBuilder CreateEmbbedingSourceClass(StringBuilder sb,
            ImmutableArray<SourceFileInfo> sources,
            string ifDirective)
        {
            if (!string.IsNullOrWhiteSpace(ifDirective))
                sb.Append("#if ").AppendLine(ifDirective);
            sb.AppendLine("namespace SourceExpander.Embedded{");
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("public class SourceFileInfo{");
            sb.AppendLine("  public string FileName{get;set;}");
            sb.AppendLine("  public string[] TypeNames{get;set;}");
            sb.AppendLine("  public string[] Usings{get;set;}");
            sb.AppendLine("  public string[] Dependencies{get;set;}");
            sb.AppendLine("  public string CodeBody{get;set;}");
            sb.AppendLine("}");
            sb.AppendLine("public class SourceFileInfoContainer{");
            sb.AppendLine("  public static readonly IReadOnlyList<SourceFileInfo> Files = new SourceFileInfo[]{");
            foreach (var source in sources)
            {
                sb.AppendLine("    new SourceFileInfo{");
                if (source.FileName is { } fileName)
                    sb.Append("      FileName = ").Append(fileName.ToLiteral()).AppendLine(",");
                if (source.CodeBody is { } body)
                    sb.Append("      CodeBody = ").Append(body.ToLiteral()).AppendLine(",");
                if (source.TypeNames is { } typeNames)
                    sb.Append("      TypeNames = new string[]{").Append(string.Join(", ",
                        typeNames.Select(s => s.ToLiteral()))).AppendLine("},");
                if (source.Usings is { } usings)
                    sb.Append("      Usings = new string[]{").Append(string.Join(", ",
                        usings.Select(s => s.ToLiteral()))).AppendLine("},");
                if (source.Dependencies is { } deps)
                    sb.Append("      Dependencies = new string[]{").Append(string.Join(", ",
                        deps.Select(s => s.ToLiteral()))).AppendLine("},");
                sb.AppendLine("    },");
            }
            sb.AppendLine("  };");
            sb.AppendLine("}");
            sb.AppendLine("}");
            if (!string.IsNullOrWhiteSpace(ifDirective))
                sb.AppendLine("#endif");
            return sb;
        }
    }
}
