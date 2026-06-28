using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace SourceExpander;

[SuppressMessage("", "CA1822")]
internal readonly partial struct SourceExpanderCommand
{
    public TextWriter? Stdout { init; private get; }
    public TextWriter? Stderr { init; private get; }
    TextWriter Output => Stdout ?? Console.Out;
    TextWriter Error => Stderr ?? Console.Error;

    private static async Task<(Compilation? Compilation, Project Project)> GetCompilation(string projectPath,
        ImmutableDictionary<string, string>? properties = null,
        CancellationToken cancellationToken = default)
    {
        properties ??= ImmutableDictionary<string, string>.Empty;
        var workspace = MSBuildWorkspace.Create(properties.SetItem("ImplicitUsings", "false"));
        var project = await workspace.OpenProjectAsync(projectPath, cancellationToken: cancellationToken);
        return (await project.GetCompilationAsync(cancellationToken), project);
    }

    private static async Task<ImmutableArray<EmbeddedData>> GetAdditionalEmbeddedData(Project project, CancellationToken cancellationToken)
    {
        const string EMBEDDED_FILE_NAME = "SourceExpander.Embedded.json";

        var builder = ImmutableArray.CreateBuilder<EmbeddedData>();
        foreach (var doc in project.AdditionalDocuments)
            if (doc.FilePath?.EndsWith(EMBEDDED_FILE_NAME, StringComparison.OrdinalIgnoreCase) is true)
            {
                try
                {
                    var obj = JsonUtil.ParseJson<EmbeddedData>(await doc.GetTextAsync(cancellationToken));
                    if (obj is not null)
                        builder.Add(obj);
                }
                catch (ParseJsonException)
                {
                }
            }
        return builder.ToImmutable();
    }
}
