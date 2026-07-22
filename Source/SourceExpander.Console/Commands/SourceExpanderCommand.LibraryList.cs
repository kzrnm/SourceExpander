using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace SourceExpander;

partial struct SourceExpanderCommand
{
    /// <summary>
    /// List embedded libraries from dependency.
    /// </summary>
    /// <param name="target">Target project(.csproj/.cs).</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    [Command("library-list")]
    public async Task LibraryList(
        [Argument] string target,
        CancellationToken cancellationToken = default)
    {
        var targetInfo = new FileInfo(target);
        if (!targetInfo.Exists)
            throw new ArgumentException("File does not exist.", nameof(target));
        var project = targetInfo.Extension == ".csproj" ? targetInfo.FullName : PathUtil.GetProjectPath(target);
        var (compilation, csProject) = await GetCompilation(project, cancellationToken: cancellationToken);

        if (compilation == null)
            throw new InvalidOperationException("Failed to get compilation");

        var libs = new HashSet<(string Name, Version EmbedderVersion)>();

        var embedded = await GetAdditionalEmbeddedData(csProject, cancellationToken);
        foreach (var e in embedded)
        {
            libs.Add((e.AssemblyName, e.EmbedderVersion));
        }

        var metadataResolver = new AssemblyMetadataResolver(compilation);
        foreach (var symbol in compilation.References
            .Select(compilation.GetAssemblyOrModuleSymbol)
            .Prepend(compilation.Assembly)
            .OfType<ISymbol>()
            .Distinct(SymbolEqualityComparer.Default))
        {
            var dict = metadataResolver.GetAssemblyMetadata(symbol);
            if (dict.TryGetValue("SourceExpander.EmbedderVersion", out var version))
            {
                libs.Add((symbol.Name, Version.Parse(version)));
            }
        }

        foreach (var (name, version) in libs.Order())
        {
            Output.WriteLine($"{name},{version}");
        }
    }
}
