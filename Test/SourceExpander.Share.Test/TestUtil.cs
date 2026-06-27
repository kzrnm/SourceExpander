using System.Text.Json;
using Microsoft.CodeAnalysis;

namespace SourceExpander.Share;

static class TestUtil
{
    public static readonly MetadataReference[] defaultMetadatas = GetDefaulMetadatas().ToArray();

    private static IEnumerable<MetadataReference> GetDefaulMetadatas()
    {
        var directory = Path.GetDirectoryName(typeof(object).Assembly.Location);
        foreach (var file in Directory.EnumerateFiles(directory, "System*.dll"))
        {
            yield return MetadataReference.CreateFromFile(file);
        }
    }

    public static IEqualityComparer<string> JsonEqualityComparer
        = EqualityComparer<string>.Create(
            (x, y) => JsonElement.DeepEquals(JsonElement.Parse(x), JsonElement.Parse(y)));

    public static IEqualityComparer<EmbeddedData> EmbeddedDataEqualityComparer
        = EqualityComparer<EmbeddedData>.Create(
            (x, y) => x.AssemblyName == y.AssemblyName
                && x.EmbedderVersion == y.EmbedderVersion
                && x.AllowUnsafe == y.AllowUnsafe
                && x.CSharpVersion == y.CSharpVersion
                && x.EmbeddedNamespaces.SequenceEqual(y.EmbeddedNamespaces)
                && x.Sources.SequenceEqual(y.Sources, SourceFileInfoEqualityComparer));
    public static IEqualityComparer<SourceFileInfo> SourceFileInfoEqualityComparer
        = EqualityComparer<SourceFileInfo>.Create(
            (x, y) => x.FileName == y.FileName
                && x.CodeBody == y.CodeBody
                && x.Dependencies.SequenceEqual(y.Dependencies)
                && x.Usings.SequenceEqual(y.Usings)
                && x.TypeNames.SequenceEqual(y.TypeNames));
}
