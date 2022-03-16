using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.MSBuild;

internal partial class SourceExpanderCommand : ConsoleAppBase
{
    [RootCommand]
    public async Task Expand(
    [Option(0, "expanding file")] string expand,
    [Option("o", "output file")] string? output = null,
    [Option("p", "csproj file")] string? project = null,
    [Option("s", "static embedding text")] string? staticEmbedding = null)
    {
        project ??= PathUtil.GetProjectPath(expand);
        project = Path.GetFullPath(project);

        var (compilation, csProject) = await GetCompilation(project);
        if (compilation is not CSharpCompilation csCompilation)
            throw new InvalidOperationException("Failed to get compilation");
        if (csProject.ParseOptions is not CSharpParseOptions parseOptions)
            throw new InvalidOperationException("Failed to get parseOptions");

        var config = new ExpandConfig(
            matchFilePatterns: new[] { new FileInfo(expand).FullName },
            staticEmbeddingText: staticEmbedding);

        var (_, code) = new EmbeddedLoader(csCompilation,
            parseOptions,
            config,
            Context.CancellationToken)
            .ExpandedCodes()
            .SingleOrDefault();

        if (code is null)
            throw new InvalidOperationException($"Failed to get {expand} in project {project}");


        if (output is null)
        {
            Console.Write(code);
        }
        else
        {
            output = Path.GetFullPath(output);

            Console.WriteLine($"expanding file: {project}");
            Console.WriteLine($"project: {expand}");
            Console.WriteLine($"output: {output}");

            await File.WriteAllTextAsync(output, code);
        }
    }

    private async Task<(Compilation? Compilation, Project Project)> GetCompilation(string projectPath, IDictionary<string, string>? properties = null)
    {
        var workspace = MSBuildWorkspace.Create(properties ?? ImmutableDictionary<string, string>.Empty);
        var project = await workspace.OpenProjectAsync(projectPath, cancellationToken: Context.CancellationToken);
        return (await project.GetCompilationAsync(Context.CancellationToken), project);
    }
}
