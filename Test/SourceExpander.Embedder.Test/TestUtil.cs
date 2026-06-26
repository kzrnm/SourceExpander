using System.Diagnostics;
using System.Runtime.CompilerServices;
using TUnit.Assertions.Should.Core;

namespace SourceExpander;

public static class TestUtil
{
    internal static IEqualityComparer<SourceFileInfo> SourceFileInfoEqualityComparer
        = EqualityComparer<SourceFileInfo>.Create(
            (x, y) => x.FileName == y.FileName
                && x.CodeBody == y.CodeBody
                && x.Dependencies.SequenceEqual(y.Dependencies)
                && x.Usings.SequenceEqual(y.Usings)
                && x.TypeNames.SequenceEqual(y.TypeNames));

    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static ShouldAssertion<IEnumerable<SourceFileInfo>> BeEquivalentTo(this ShouldCollectionSource<SourceFileInfo> should, IEnumerable<SourceFileInfo> expected)
    {
        return should.BeEquivalentTo(expected, SourceFileInfoEqualityComparer);
    }
}
