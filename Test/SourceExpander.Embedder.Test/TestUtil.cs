namespace SourceExpander;

public static class TestUtil
{
    internal static IEqualityComparer<EmbeddedData> EmbeddedDataEqualityComparer
        = EqualityComparer<EmbeddedData>.Create(
            (x, y) => x.AssemblyName == y.AssemblyName
                && x.EmbedderVersion == y.EmbedderVersion
                && x.CSharpVersion == y.CSharpVersion
                && x.AllowUnsafe == y.AllowUnsafe
                && x.Sources.SequenceEqual(y.Sources, SourceFileInfoEqualityComparer)
                && x.EmbeddedNamespaces.SequenceEqual(y.EmbeddedNamespaces));

    internal static IEqualityComparer<SourceFileInfo> SourceFileInfoEqualityComparer
        = EqualityComparer<SourceFileInfo>.Create(
            (x, y) => x.FileName == y.FileName
                && x.CodeBody == y.CodeBody
                && x.Dependencies.SequenceEqual(y.Dependencies)
                && x.Usings.SequenceEqual(y.Usings)
                && x.TypeNames.SequenceEqual(y.TypeNames));
}
