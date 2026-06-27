using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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
    internal static async Task ShouldContainKeyAndValue([NotNull] this IDictionary<string, DependencyResult>? dictionary, string key, DependencyResult val)
    {
        await dictionary.Should().NotBeNull();
        _ = dictionary!.GetType();
        await dictionary.Should().ContainKey(key);
        var dependencyResult = dictionary[key];

        await dependencyResult.Should().NotBeNull();

        using (Assert.Multiple())
        {
            await dependencyResult.FileName.Should().BeEqualTo(val.FileName);
            await dependencyResult.Dependencies.Should().BeEquivalentTo(val.Dependencies);
            await dependencyResult.TypeNames.Should().BeEquivalentTo(val.TypeNames);
        }
    }
}
