using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
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
        public void Initialize(GeneratorInitializationContext context)
            => context.RegisterForPostInitialization(ctx =>
            {
                foreach (var (hintName, sourceText) in CompileTimeTypeMaker.Sources)
                    ctx.AddSource(hintName, sourceText);
            });

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types")]
        public void Execute(GeneratorExecutionContext context)
        {
            try
            {
#if DEBUG
                if (!Debugger.IsAttached)
                {
                    //System.Diagnostics.Debugger.Launch();
                }
#endif

                if (context.Compilation is not CSharpCompilation compilation) return;
                if (!compilation.SyntaxTrees.Any()) return;
                if (compilation.GetDiagnostics(context.CancellationToken).HasCompilationError()) return;

                var configFile = context.AdditionalFiles
                        .FirstOrDefault(a =>
                        StringComparer.OrdinalIgnoreCase.Compare(Path.GetFileName(a.Path), CONFIG_FILE_NAME) == 0);

                context.CancellationToken.ThrowIfCancellationRequested();
                EmbedderConfig config;
                if (configFile?.GetText(context.CancellationToken) is { } configText)
                {
                    try
                    {
                        config = EmbedderConfig.Parse(configText);
                        foreach (var p in config.ObsoleteConfigProperties)
                            context.ReportDiagnostic(
                                DiagnosticDescriptors.EMBED0011_ObsoleteConfigProperty(p.Name, p.Instead));
                    }
                    catch (ParseConfigException e)
                    {
                        context.ReportDiagnostic(
                            DiagnosticDescriptors.EMBED0003_ParseConfigError(configFile.Path, e.Message));
                        return;
                    }
                }
                else config = new();

                if (!config.Enabled)
                    return;

                context.CancellationToken.ThrowIfCancellationRequested();
                var embeddingContext = new EmbeddingContext(
                    compilation,
                    (CSharpParseOptions)context.ParseOptions,
                    new DiagnosticReporter(context),
                    config,
                    context.CancellationToken);

                var resolver = new EmbeddingResolver(embeddingContext);
                var resolvedSources = resolver.ResolveFiles();

                if (resolvedSources.Length == 0)
                    return;

                if (config.EmbeddingSourceClass.Enabled)
                    context.AddSource(
                        "EmbeddingSourceClass.cs",
                        CreateEmbbedingSourceClass(resolvedSources, config.EmbeddingSourceClass.ClassName));

                context.AddSource(
                    "EmbeddedSourceCode.Metadata.cs", CreateMetadataSource(resolver.EnumerateAssemblyMetadata()));

            }
            catch (OperationCanceledException)
            {
                Trace.WriteLine(nameof(EmbedderGenerator) + "." + nameof(Execute) + "is Canceled.");
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.ToString());
                context.ReportDiagnostic(
                    DiagnosticDescriptors.EMBED0001_UnknownError(e.Message));
            }
        }

        private static SourceText CreateMetadataSource(IEnumerable<(string Key, string Value)> metadatas)
        {
            StringBuilder sb = new();
            sb.AppendLine("using System.Reflection;");
            foreach (var (key, value) in metadatas)
            {
                sb.Append("[assembly: AssemblyMetadataAttribute(")
                  .Append(key.ToLiteral()).Append(",")
                  .Append(value.ToLiteral())
                  .AppendLine(")]");
            }
            return SourceText.From(sb.ToString(), Encoding.UTF8);
        }

        private static SourceText CreateEmbbedingSourceClass(
            ImmutableArray<SourceFileInfo> sources,
            string className)
        {
            StringBuilder sb = new();
            sb.AppendLine("namespace SourceExpander.Embedded{");
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("public class " + className + "{");
            sb.AppendLine("public class SourceFileInfo{");
            sb.AppendLine("  public string FileName{get;set;}");
            sb.AppendLine("  public string[] TypeNames{get;set;}");
            sb.AppendLine("  public string[] Usings{get;set;}");
            sb.AppendLine("  public string[] Dependencies{get;set;}");
            sb.AppendLine("  public string CodeBody{get;set;}");
            sb.AppendLine("}");
            sb.AppendLine("  public static readonly IReadOnlyList<SourceFileInfo> Files = new SourceFileInfo[]{");
            foreach (var source in sources)
            {
                sb.AppendLine("    new SourceFileInfo{");
                if (source.FileName is { } fileName)
                    sb.Append("      FileName = ").Append(fileName.ToLiteral()).AppendLine(",");
                if (source.CodeBody is { } body)
                    sb.Append("      CodeBody = ").Append(body.ToLiteral()).AppendLine(",");
                if (source.TypeNames is { } typeNames)
                {
                    sb.AppendLine("      TypeNames = new string[]{");
                    foreach (var ty in typeNames.Select(s => s.ToLiteral()))
                        sb.Append("        ").Append(ty).AppendLine(",");
                    sb.AppendLine("      },");
                }
                if (source.Usings is { } usings)
                {
                    sb.AppendLine("      Usings = new string[]{");
                    foreach (var u in usings.OrderBy(s => s).Select(s => s.ToLiteral()))
                        sb.Append("        ").Append(u).AppendLine(",");
                    sb.AppendLine("      },");
                }
                if (source.Dependencies is { } deps)
                {
                    sb.AppendLine("      Dependencies = new string[]{");
                    foreach (var d in deps.Select(s => s.ToLiteral()))
                        sb.Append("        ").Append(d).AppendLine(",");
                    sb.AppendLine("      },");
                }
                sb.AppendLine("    },");
            }
            sb.AppendLine("  };");
            sb.AppendLine("}");
            sb.AppendLine("}");
            return SourceText.From(sb.ToString(), Encoding.UTF8);
        }
    }
}
