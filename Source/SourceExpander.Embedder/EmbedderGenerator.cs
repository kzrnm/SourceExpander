using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using SourceExpander.Roslyn;

namespace SourceExpander
{
    [Generator]
    public class EmbedderGenerator : IIncrementalGenerator
    {
        private const string CONFIG_FILE_NAME = "SourceExpander.Embedder.Config.json";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterPostInitializationOutput(ctx =>
            {
                foreach (var (hintName, sourceText) in CompileTimeTypeMaker.Sources)
                    ctx.AddSource(hintName, sourceText);
            });

            IncrementalValueProvider<(EmbedderConfig Config, ImmutableArray<Diagnostic> Diagnostic)> configProvider
                = context.AdditionalTextsProvider
                .Where(a => StringComparer.OrdinalIgnoreCase.Compare(Path.GetFileName(a.Path), CONFIG_FILE_NAME) == 0)
                .Collect().Select(ParseAdditionalTexts);

            var source = context.CompilationProvider
                .Combine(context.ParseOptionsProvider)
                .Combine(configProvider);

            context.RegisterImplementationSourceOutput(source, (ctx, source) =>
            {
                var ((compilationOrig, parseOptions), (config, configDiagnostic)) = source;

                try
                {
                    foreach (var diag in configDiagnostic)
                    {
                        ctx.ReportDiagnostic(diag);
                    }

                    if (compilationOrig is not CSharpCompilation compilation) return;
                    if (!compilation.SyntaxTrees.Any()) return;
                    if (compilation.GetDiagnostics(ctx.CancellationToken).HasCompilationError()) return;

                    if (!config.Enabled)
                        return;

                    ctx.CancellationToken.ThrowIfCancellationRequested();
                    var embeddingContext = new EmbeddingContext(
                        compilation,
                        (CSharpParseOptions)parseOptions,
                        new SourceProductionContextDiagnosticReporter(ctx),
                        config,
                        ctx.CancellationToken);

                    var resolver = new EmbeddingResolver(embeddingContext);
                    var resolvedSources = resolver.ResolveFiles();

                    if (resolvedSources.Length == 0)
                        return;

                    if (config.EmbeddingSourceClass.Enabled)
                        ctx.AddSource(
                            "EmbeddingSourceClass.cs",
                            CreateEmbbedingSourceClass(resolvedSources, config.EmbeddingSourceClass.ClassName));

                    ctx.AddSource(
                        "EmbeddedSourceCode.Metadata.cs", CreateMetadataSource(resolver.EnumerateAssemblyMetadata()));
                }
                catch (OperationCanceledException)
                {
                    Trace.WriteLine(nameof(EmbedderGenerator) + "." + nameof(Execute) + "is Canceled.");
                }
                catch (Exception e)
                {
                    Trace.WriteLine(e.ToString());
                    ctx.ReportDiagnostic(
                        DiagnosticDescriptors.EMBED0001_UnknownError(e.Message));
                }
            });
        }
        private static (EmbedderConfig Config, ImmutableArray<Diagnostic> Diagnostic) ParseAdditionalTexts(ImmutableArray<AdditionalText> additionalTexts, CancellationToken cancellationToken = default)
        {
            var at = additionalTexts.FirstOrDefault();

            if (at?.GetText(cancellationToken)?.ToString() is not { } configText)
                return (new EmbedderConfig(), ImmutableArray<Diagnostic>.Empty);

            try
            {
                var config = EmbedderConfig.Parse(configText);
                var diagnosticsBuilder = ImmutableArray.CreateBuilder<Diagnostic>();

                if (config.ObsoleteConfigProperties.Any())
                {
                    foreach (var p in config.ObsoleteConfigProperties)
                    {
                        diagnosticsBuilder.Add(
                            DiagnosticDescriptors.EMBED0011_ObsoleteConfigProperty(DiagnosticDescriptors.AdditionalFileLocation(at.Path), at.Path, p.Name, p.Instead));
                    }
                }
                return (config, diagnosticsBuilder.ToImmutable());
            }
            catch (ParseJsonException e)
            {
                return (new EmbedderConfig(), ImmutableArray.Create(DiagnosticDescriptors.EMBED0003_ParseConfigError(at.Path, e.Message)));
            }
        }

        public void Initialize(GeneratorInitializationContext context)
            => context.RegisterForPostInitialization(ctx =>
            {
                foreach (var (hintName, sourceText) in CompileTimeTypeMaker.Sources)
                    ctx.AddSource(hintName, sourceText);
            });

        public void Execute(GeneratorExecutionContext context)
        {
            try
            {
                if (context.Compilation is not CSharpCompilation compilation) return;
                if (!compilation.SyntaxTrees.Any()) return;
                if (compilation.GetDiagnostics(context.CancellationToken).HasCompilationError()) return;

                var configFile = context.AdditionalFiles
                        .FirstOrDefault(a =>
                        StringComparer.OrdinalIgnoreCase.Compare(Path.GetFileName(a.Path), CONFIG_FILE_NAME) == 0);

                context.CancellationToken.ThrowIfCancellationRequested();
                EmbedderConfig config;
                if (configFile?.GetText(context.CancellationToken)?.ToString() is { } configText)
                {
                    try
                    {
                        config = EmbedderConfig.Parse(configText);
                        if (config.ObsoleteConfigProperties.Any())
                        {
                            foreach (var p in config.ObsoleteConfigProperties)
                            {
                                context.ReportDiagnostic(
                                    DiagnosticDescriptors.EMBED0011_ObsoleteConfigProperty(
                                        DiagnosticDescriptors.AdditionalFileLocation(configFile.Path),
                                        configFile.Path, p.Name, p.Instead));
                            }
                        }
                    }
                    catch (ParseJsonException e)
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
                    new GeneratorExecutionContextDiagnosticReporter(context),
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
