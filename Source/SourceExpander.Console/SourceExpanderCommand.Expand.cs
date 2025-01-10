using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

partial struct SourceExpanderCommand
{
    /// <summary>
    /// Expand embedded source.
    /// </summary>
    /// <param name="expand">Expanding file.</param>
    /// <param name="output">-o,Output file</param>
    /// <param name="project">-p,csproj file</param>
    /// <param name="staticEmbedding">-s,Static embedding text</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    [Command("")]
    public async Task Expand(
        [Argument] string expand,
        string? output = null,
        string? project = null,
        string? staticEmbedding = null,
        CancellationToken cancellationToken = default)
    {
        project ??= PathUtil.GetProjectPath(expand);
        project = Path.GetFullPath(project);

        var (compilation, csProject) = await GetCompilation(project, cancellationToken: cancellationToken);
        if (compilation is not CSharpCompilation csCompilation)
            throw new InvalidOperationException("Failed to get compilation");
        if (csProject.ParseOptions is not CSharpParseOptions parseOptions)
            throw new InvalidOperationException("Failed to get parseOptions");

        var config = new ExpandConfig(
            matchFilePatterns: [new FileInfo(expand).FullName],
            staticEmbeddingText: staticEmbedding);

        var (_, code) = new EmbeddedLoader(csCompilation,
            parseOptions,
            config,
            cancellationToken)
            .ExpandedCodes()
            .SingleOrDefault();

        if (code is null)
            throw new InvalidOperationException($"Failed to get {expand} in project {project}");


        if (output is null)
        {
            Output.Write(code);
        }
        else
        {
            output = Path.GetFullPath(output);

            Output.WriteLine($"expanding file: {project}");
            Output.WriteLine($"project: {expand}");
            Output.WriteLine($"output: {output}");

            await File.WriteAllTextAsync(output, code, cancellationToken: cancellationToken);
        }
    }

    /// <summary>
    /// Show expanded codes json.
    /// </summary>
    /// <param name="project">Target project(.csproj).</param>
    /// <param name="staticEmbedding">-s,Static embedding text.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    [Command("expand-all")]
    public async Task ExpandAll(
        [Argument] string project,
        string? staticEmbedding = null,
        CancellationToken cancellationToken = default)
    {
        project = Path.GetFullPath(project);

        var (compilation, csProject) = await GetCompilation(project, cancellationToken: cancellationToken);
        if (compilation is not CSharpCompilation csCompilation)
            throw new InvalidOperationException("Failed to get compilation");
        if (csProject.ParseOptions is not CSharpParseOptions parseOptions)
            throw new InvalidOperationException("Failed to get parseOptions");

        var config = new ExpandConfig(staticEmbeddingText: staticEmbedding);

        var expanded = new EmbeddedLoader(csCompilation,
            parseOptions,
            config,
            cancellationToken)
            .ExpandedCodes();

        var result = JsonSerializer.Serialize(expanded.Select(t => new
        {
            t.SyntaxTree.FilePath,
            t.ExpandedCode,
        }), DefaultSerializerOptions);
        Output.WriteLine(result);
    }
}
