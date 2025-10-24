using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;

namespace SourceExpander;

public static class TestUtil
{
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void ShouldBeEquivalentTo([NotNull] this IEnumerable<SourceFileInfo> actual, IEnumerable<SourceFileInfo> expected)
    {
        actual.ShouldNotBeNull();
        actual.Count().ShouldBe(expected.Count());

        foreach (var (a, e) in Enumerable.Zip(actual, expected))
        {
            a.ShouldBeEquivalentTo(e);

            a.ShouldSatisfyAllConditions([
                a => a.FileName.ShouldBe(e.FileName),
                a => a.CodeBody.ShouldBe(e.CodeBody),
                a => a.Dependencies.Order().ShouldBe(e.Dependencies.Order()),
                a => a.TypeNames.Order().ShouldBe(e.TypeNames.Order()),
                a => a.Usings.Order().ShouldBe(e.Usings.Order()),
            ]);
        }
    }
}
