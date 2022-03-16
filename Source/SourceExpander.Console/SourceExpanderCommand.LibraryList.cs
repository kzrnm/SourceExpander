using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

internal partial class SourceExpanderCommand : ConsoleAppBase
{
    [Command("library-list", @"Show embedded libraries list from dependency

<assembly name>,<version>")]
    public async Task LibraryList(
        [Option(0, "target project(.csproj/.cs)")] string target)
    {
        var targetInfo = new FileInfo(target);
        if (!targetInfo.Exists)
            throw new ArgumentException("File does not exist.", nameof(target));
        var project = targetInfo.Extension == ".csproj" ? targetInfo.FullName : PathUtil.GetProjectPath(target);
        var (compilation, _) = await GetCompilation(project);

        if (compilation is not CSharpCompilation)
            throw new InvalidOperationException("Failed to get compilation");

        var metadataResolver = new AssemblyMetadataResolver(compilation);
        foreach (var symbol in compilation.References
            .Select(compilation.GetAssemblyOrModuleSymbol)
            .Prepend(compilation.Assembly)
            .OfType<ISymbol>())
        {
            var dict = metadataResolver.GetAssemblyMetadata(symbol);
            if (dict.TryGetValue("SourceExpander.EmbedderVersion", out var version))
            {
                Console.WriteLine($"{symbol.Name},{version}");
            }
        }
    }
}
