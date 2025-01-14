using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

[SuppressMessage("", "CA1822")]
internal readonly partial struct SourceExpanderCommand
{
    public TextWriter? Stdout { init; private get; }
    public TextWriter? Stderr { init; private get; }
    TextWriter Output => Stdout ?? Console.Out;
    TextWriter Error => Stderr ?? Console.Error;

    private static readonly JsonSerializerOptions DefaultSerializerOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    private static async Task<(Compilation? Compilation, Project Project)> GetCompilation(string projectPath,
        IDictionary<string, string>? properties = null,
        CancellationToken cancellationToken = default)
    {
        var workspace = MSBuildWorkspace.Create(properties ?? ImmutableDictionary<string, string>.Empty);
        var project = await workspace.OpenProjectAsync(projectPath, cancellationToken: cancellationToken);
        return (await project.GetCompilationAsync(cancellationToken), project);
    }
}
