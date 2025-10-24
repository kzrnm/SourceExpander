using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace SourceExpander;

public static class TestUtil
{
    static string ThisFileDir([CallerFilePath] string path = "") => Path.GetDirectoryName(path)!;
    public static string TestProjectDirectory => ThisFileDir();
    public static string SourceDirectory
        => Path.GetFullPath(Path.Combine(ThisFileDir(), "../../Source"));

    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void ShouldContainKeyAndValue([NotNull] this IDictionary<string, DependencyResult>? dictionary, string key, DependencyResult val)
    {
        dictionary.ShouldNotBeNull();
        dictionary.ShouldContainKey(key);
        var actual = dictionary[key];

        actual.ShouldNotBeNull();
        actual.ShouldSatisfyAllConditions([
            r => r.FileName.ShouldBe(val.FileName),
            r => r.Dependencies.Order().ShouldBe(val.Dependencies.Order()),
            r => r.TypeNames.Order().ShouldBe(val.TypeNames.Order()),
        ]);
    }
}
