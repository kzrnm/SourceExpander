using System;
using System.IO;
using System.Linq;
using System.Runtime.Loader;
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
    [Option("s", "static embedding text")] string? staticEmbedding = null
)
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

    private async Task<(Compilation? Compilation, Project Project)> GetCompilation(string projectPath)
    {
        var workspace = MSBuildWorkspace.Create();
        var project = await workspace.OpenProjectAsync(projectPath, cancellationToken: Context.CancellationToken);
        return (await project.GetCompilationAsync(Context.CancellationToken), project);
    }
}
