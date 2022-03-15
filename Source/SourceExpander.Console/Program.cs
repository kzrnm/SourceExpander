using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.Loader;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using System.Threading.Tasks;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.MSBuild;

System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
System.Globalization.CultureInfo.CurrentUICulture = System.Globalization.CultureInfo.InvariantCulture;

var instance = MSBuildLocator.RegisterDefaults();
AssemblyLoadContext.Default.Resolving += (assemblyLoadContext, assemblyName) =>
{
    var path = Path.Combine(instance.MSBuildPath, assemblyName.Name + ".dll");
    if (File.Exists(path))
    {
        return assemblyLoadContext.LoadFromAssemblyPath(path);
    }

    return null;
};
ConsoleApp.Run<SourceExpanderCommand>(args);

internal class SourceExpanderCommand : ConsoleAppBase
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
            throw new InvalidOperationException("Failed to get parseOptions compilation");
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
            if (!Version.TryParse(
                metadataDict.GetValueOrDefault("SourceExpander.EmbedderVersion")
                ?? metadataDict.GetValueOrDefault("SourceExpander.ExpanderVersion"),
                out var version) || version <= new Version(4, 2, 0, 2))
            {
                if (version is null)
                    await Console.Error.WriteLineAsync("needs SourceExpander 5.0.0 or newer");
                else
                    await Console.Error.WriteLineAsync($"needs SourceExpander 5.0.0 or newer, Current: {version}");

                Environment.Exit(1);
                return;
            }
        }
        var metadatas = metadataResolver.GetEmbeddedSourceFiles(true, Context.CancellationToken)
            .ToArray();

        var infos = metadatas.SelectMany(t => t.Data.Sources);
        if (metadatas.FirstOrDefault(t => t.Name == compilation.AssemblyName) is not { Data.Sources.Length: > 0 })
        {
            infos = infos.Concat(
                new EmbeddedLoader(csCompilation,
                parseOptions,
                new ExpandConfig(),
                Context.CancellationToken)
                .Dependencies()
                .Select(t => new SourceFileInfo(t.FilePath, null, null, t.Dependencies, null)));
        }
        var result = JsonSerializer.Serialize(infos, new JsonSerializerOptions
        {
            Converters = { new SourceFileInfoConverter() },
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(UnicodeRanges.BasicLatin),
        });
        Console.WriteLine(result);

    }
    private class SourceFileInfoConverter : JsonConverter<SourceFileInfo>
    {
        public override SourceFileInfo Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options) => throw new NotSupportedException();
        public override void Write(
            Utf8JsonWriter writer,
            SourceFileInfo info,
            JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("FileName", info.FileName);
            writer.WriteStartArray("Dependencies");
            foreach (var d in info.Dependencies)
                writer.WriteStringValue(d);
            writer.WriteEndArray();
            writer.WriteStartArray("DefinedTypes");
            foreach (var d in info.TypeNames)
                writer.WriteStringValue(d);
            writer.WriteEndArray();
            writer.WriteEndObject();
        }
    }

    private async Task<(Compilation? Compilation, Project Project)> GetCompilation(string projectPath, IDictionary<string, string>? properties = null)
    {
        var workspace = MSBuildWorkspace.Create(properties ?? ImmutableDictionary<string, string>.Empty);
        var project = await workspace.OpenProjectAsync(projectPath, cancellationToken: Context.CancellationToken);
        return (await project.GetCompilationAsync(Context.CancellationToken), project);
    }
}
