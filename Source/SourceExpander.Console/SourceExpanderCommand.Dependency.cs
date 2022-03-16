using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

internal partial class SourceExpanderCommand : ConsoleAppBase
{
    [Command("dependency", "Show dependency json")]
    public async Task Dependency(
        [Option(0, "target project(.csproj/.cs)")] string target,
        [Option("p", "file name as full path")] bool fullFilePath = false)
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

        var (compilation, csProject) = await GetCompilation(project, props);
        if (compilation is not CSharpCompilation csCompilation)
            throw new InvalidOperationException("Failed to get parseOptions compilation");
        if (csProject.ParseOptions is not CSharpParseOptions parseOptions)
            throw new InvalidOperationException("Failed to get parseOptions");

        var metadataResolver = new AssemblyMetadataResolver(compilation);
        var metadataDict = metadataResolver.GetAssemblyMetadata();
        {
            if (Version.TryParse(
                metadataDict.GetValueOrDefault("SourceExpander.EmbedderVersion"),
                out var version) && version < new Version(5, 0, 0, 0))
            {
                if (version is null)
                    await Console.Error.WriteLineAsync("needs SourceExpander.Embedder 5.0.0 or newer");
                else
                    await Console.Error.WriteLineAsync($"needs SourceExpander.Embedder 5.0.0 or newer, Current: {version}");

                Environment.Exit(1);
                return;
            }
        }
        var metadatas = metadataResolver.GetEmbeddedSourceFiles(true, Context.CancellationToken)
            .ToArray();

        var infos = metadatas.SelectMany(t => t.Data.Sources)
            .Select(DependencySourceFileInfo.FromInfo);
        if (metadatas.FirstOrDefault(t => t.Name == compilation.AssemblyName) is not { Data.Sources.Length: > 0 })
        {
            infos = infos.Concat(
                new EmbeddedLoader(csCompilation,
                parseOptions,
                new ExpandConfig(),
                Context.CancellationToken)
                .Dependencies()
                .Select(t => new DependencySourceFileInfo(t.FilePath, t.Dependencies, Array.Empty<string>())));
        }
        var result = JsonSerializer.Serialize(infos, new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        });
        Console.WriteLine(result);
    }
    private record DependencySourceFileInfo(string FileName, IEnumerable<string> Dependencies, IEnumerable<string> DefinedTypes)
    {
        public static DependencySourceFileInfo FromInfo(SourceFileInfo info)
            => new(info.FileName, info.Dependencies, info.TypeNames);
    }
}
