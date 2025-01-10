using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
    /// Show dependency json.
    /// </summary>
    /// <param name="target">Target project(.csproj/.cs).</param>
    /// <param name="fullFilePath">-p,File name as full path.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    [Command("dependency")]
    public async Task Dependency(
        [Argument] string target,
        bool fullFilePath = false,
        CancellationToken cancellationToken = default)
    {
        var targetInfo = new FileInfo(target);
        if (!targetInfo.Exists)
            throw new ArgumentException("File does not exist.", nameof(target));
        var project = targetInfo.Extension == ".csproj" ? targetInfo.FullName : PathUtil.GetProjectPath(target);

        var props = new Dictionary<string, string>
        {
            { "SourceExpander_Embedder_EmbeddingType", "Raw" },
        };
        if (fullFilePath)
            props.Add("SourceExpander_Embedder_EmbeddingFileNameType", "FullPath");

        var (compilation, csProject) = await GetCompilation(project, props, cancellationToken: cancellationToken);
        if (compilation is not CSharpCompilation csCompilation)
            throw new InvalidOperationException("Failed to get compilation");
        if (csProject.ParseOptions is not CSharpParseOptions parseOptions)
            throw new InvalidOperationException("Failed to get parseOptions");

        var metadataResolver = new AssemblyMetadataResolver(compilation);
        var metadataDict = metadataResolver.GetAssemblyMetadata(compilation.Assembly);
        {
            if (Version.TryParse(
                metadataDict.GetValueOrDefault("SourceExpander.EmbedderVersion"),
                out var version) && version < new Version(5, 0, 0, 0))
            {
                if (version is null)
                    await Error.WriteLineAsync("needs SourceExpander.Embedder 5.0.0 or newer");
                else
                    await Error.WriteLineAsync($"needs SourceExpander.Embedder 5.0.0 or newer, Current: {version}");

                Environment.Exit(1);
                return;
            }
        }
        var metadatas = metadataResolver
            .GetEmbeddedSourceFiles(true, cancellationToken)
            .ToArray();

        var infos = metadatas.SelectMany(t => t.Data.Sources)
            .Select(t => new DependencyResult(
                FileName: t.FileName,
                Dependencies: t.Dependencies,
                TypeNames: t.TypeNames
            ));
        if (metadatas.FirstOrDefault(t => t.Name == compilation.AssemblyName) is not { Data.Sources.Length: > 0 })
        {
            infos = infos.Concat(
                new EmbeddedLoader(csCompilation,
                parseOptions,
                new ExpandConfig(),
                cancellationToken)
                .Dependencies()
                .Select(t => new DependencyResult(
                    FileName: t.FilePath,
                    Dependencies: t.Dependencies,
                    TypeNames: Enumerable.Empty<string>()
                )));
        }
        var result = JsonSerializer.Serialize(infos, DefaultSerializerOptions);
        Output.WriteLine(result);
    }
}
