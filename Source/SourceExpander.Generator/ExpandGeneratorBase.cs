using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using SourceExpander.Roslyn;

namespace SourceExpander
{
    public class ExpandGeneratorBase
    {
        internal void Execute(IContextWrappter ctx, CSharpCompilation compilation, ParseOptions parseOptions, ExpandConfig config, ImmutableArray<Diagnostic> configDiagnostic)
        {
            try
            {
                if (!config.Enabled)
                    return;
                if ((CSharpParseOptions)parseOptions is { LanguageVersion: <= LanguageVersion.CSharp3 })
                    return;

                foreach (var diag in configDiagnostic)
                {
                    ctx.ReportDiagnostic(diag);
                }

                ctx.CancellationToken.ThrowIfCancellationRequested();
                var loader = new EmbeddedLoaderWithDiagnostic(compilation, (CSharpParseOptions)parseOptions, ctx, config, ctx.CancellationToken);
                if (loader.IsEmbeddedEmpty)
                    ctx.ReportDiagnostic(DiagnosticDescriptors.EXPAND0003_NotFoundEmbedded());

                if (config.MetadataExpandingFile is { Length: > 0 } metadataExpandingFile)
                {
                    try
                    {
                        var (_, code) = loader.ExpandedCodes()
                           .First(t => t.filePath.IndexOf(metadataExpandingFile, StringComparison.OrdinalIgnoreCase) >= 0);

                        ctx.AddSource("SourceExpander.Metadata.cs",
                            CreateMetadataSource(new (string name, string code)[] {
                                ("SourceExpander.Expanded.Default", code),
                            }));
                    }
                    catch (InvalidOperationException)
                    {
                        ctx.ReportDiagnostic(DiagnosticDescriptors.EXPAND0009_MetadataEmbeddingFileNotFound(metadataExpandingFile));
                    }
                }
                var expandedCode = CreateExpanded(loader.ExpandedCodes());

                ctx.CancellationToken.ThrowIfCancellationRequested();
                ctx.AddSource("SourceExpander.Expanded.cs", expandedCode);
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

        static SourceText CreateExpanded(IEnumerable<(string filePath, string expandedCode)> expanded)
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
            foreach (var (path, code) in expanded)
            {
                var filePathLiteral = path.ToLiteral();
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

        internal static (ExpandConfig Config, ImmutableArray<Diagnostic> Diagnostic) ParseAdditionalTexts(AdditionalText? additionalText, CancellationToken cancellationToken = default)
        {
            if (additionalText?.GetText(cancellationToken)?.ToString() is not { } configText)
                return (new ExpandConfig(), ImmutableArray<Diagnostic>.Empty);

            try
            {
                return (ExpandConfig.Parse(configText), ImmutableArray<Diagnostic>.Empty);
            }
            catch (ParseJsonException e)
            {
                return (new ExpandConfig(), ImmutableArray.Create(DiagnosticDescriptors.EXPAND0007_ParseConfigError(additionalText.Path, e.Message)));
            }
        }
    }
}
