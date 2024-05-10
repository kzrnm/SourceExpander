using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using SourceExpander.Roslyn;

namespace SourceExpander
{
    public class ExpandGeneratorBase
    {
        internal void Execute(IContextWrappter ctx, CSharpCompilation compilation, CSharpParseOptions parseOptions, ExpandConfig config, ImmutableArray<Diagnostic> configDiagnostic)
        {
            try
            {
                if (!config.Enabled)
                    return;
                if (parseOptions is { LanguageVersion: <= LanguageVersion.CSharp3 })
                    return;

                foreach (var diag in configDiagnostic)
                {
                    ctx.ReportDiagnostic(diag);
                }

                ctx.CancellationToken.ThrowIfCancellationRequested();
                var loader = new EmbeddedLoaderWithDiagnostic(compilation, parseOptions, ctx, config, ctx.CancellationToken);
                if (loader.IsEmbeddedEmpty)
                    ctx.ReportDiagnostic(DiagnosticDescriptors.EXPAND0003_NotFoundEmbedded());

                var metadata = new List<(string name, string code)>();
                if (config.MetadataExpandingFile is { Length: > 0 } metadataExpandingFile)
                {
                    try
                    {
                        var (_, code) = loader.ExpandedCodes()
                           .First(rt => rt.SyntaxTree.FilePath.IndexOf(metadataExpandingFile, StringComparison.OrdinalIgnoreCase) >= 0);

                        metadata.Add(("SourceExpander.Expanded.Default", code));
                    }
                    catch (InvalidOperationException)
                    {
                        ctx.ReportDiagnostic(DiagnosticDescriptors.EXPAND0009_MetadataEmbeddingFileNotFound(metadataExpandingFile));
                    }
                }
                metadata.Add(("SourceExpander.ExpanderVersion", AssemblyUtil.AssemblyVersion.ToString()));
                ctx.AddSource("SourceExpander.Metadata.cs", CreateMetadataSource(metadata));


                if (config.ExpandingAll)
                {
                    ctx.CancellationToken.ThrowIfCancellationRequested();
                    ctx.AddSource("SourceExpander.ExpandingAll.cs", loader.ExpandAllForTesting(ctx.CancellationToken));
                }

                var expandedCodes = loader.ExpandedCodes();
                ctx.CancellationToken.ThrowIfCancellationRequested();

                var expanded = CreateExpanded(expandedCodes);
                ctx.CancellationToken.ThrowIfCancellationRequested();
                ctx.AddSource("SourceExpander.Expanded.cs", expanded);

                if (!compilation.Options.AllowUnsafe)
                {
                    foreach (var (tree, code) in expandedCodes)
                    {
                        if (code == null) continue;

                        var expandedTree = CSharpSyntaxTree.ParseText(code, parseOptions, cancellationToken: ctx.CancellationToken);
                        var root = expandedTree.GetRoot(ctx.CancellationToken);
                        if (root.DescendantTokens().Any(t => t.IsKind(SyntaxKind.UnsafeKeyword)))
                        {
                            ctx.ReportDiagnostic(DiagnosticDescriptors.EXPAND0010_UnsafeBlock(tree.FilePath));
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Trace.WriteLine(nameof(ExpandGenerator) + "." + nameof(Execute) + "is Canceled.");
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.ToString());
                ctx.ReportDiagnostic(
                    DiagnosticDescriptors.EXPAND0001_UnknownError(e.Message));
            }
        }

        static SourceText CreateExpanded(IEnumerable<ExpandedResult> expanded)
        {
            static void CreateSourceCodeLiteral(StringBuilder sb, string pathLiteral, string codeLiteral)
                => sb.Append("SourceCode.FromDictionary(new Dictionary<string,object>{")
                  .AppendDicElement("\"path\"", pathLiteral)
                  .AppendDicElement("\"code\"", codeLiteral)
                  .Append("})");

            var sb = new StringBuilder();
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("namespace SourceExpander.Expanded{");
            sb.AppendLine("public static class ExpandedContainer{");
            sb.AppendLine("public static IReadOnlyDictionary<string, SourceCode> Files {get{ return _Files; }}");
            sb.AppendLine("private static Dictionary<string, SourceCode> _Files = new Dictionary<string, SourceCode>{");
            foreach (var (tree, code) in expanded)
            {
                var filePathLiteral = tree.FilePath.ToLiteral();
                sb.AppendDicElement(filePathLiteral, sb => CreateSourceCodeLiteral(sb, filePathLiteral, code.ToLiteral()));
                sb.AppendLine();
            }
            sb.AppendLine("};");
            sb.AppendLine("}}");
            return SourceText.From(sb.ToString(), Encoding.UTF8);
        }

        static SourceText CreateMetadataSource(IEnumerable<(string Key, string Value)> metadatas)
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

        internal static (ExpandConfig Config, ImmutableArray<Diagnostic> Diagnostic) ParseAdditionalTextAndAnalyzerOptions(AdditionalText? additionalText, AnalyzerConfigOptionsProvider analyzerConfigOptionsProvider, CancellationToken cancellationToken = default)
        {
            var isDesignTimeBuild = StringComparer.OrdinalIgnoreCase.Equals(
                analyzerConfigOptionsProvider.GlobalOptions.GetOrNull("build_property.DesignTimeBuild"),
                "true");
            if (isDesignTimeBuild)
                return (new ExpandConfig(false), ImmutableArray<Diagnostic>.Empty);

            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                var config = ExpandConfig.Parse(additionalText?.GetText(cancellationToken)?.ToString(), analyzerConfigOptionsProvider.GlobalOptions);
                var diagnosticsBuilder = ImmutableArray.CreateBuilder<Diagnostic>();
                return (config, ImmutableArray<Diagnostic>.Empty);
            }
            catch (ParseJsonException e)
            {
                return (new ExpandConfig(), ImmutableArray.Create(DiagnosticDescriptors.EXPAND0007_ParseConfigError(additionalText?.Path, e.Message)));
            }
        }
    }
}
