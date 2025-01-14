using System.IO;
using System.Runtime.CompilerServices;

namespace SourceExpander;

public static class TestUtil
{
    static string ThisFileDir([CallerFilePath] string path = "") => Path.GetDirectoryName(path)!;
    public static string TestProjectDirectory => ThisFileDir();
    public static string SourceDirectory
        => Path.GetFullPath(Path.Combine(ThisFileDir(), "../../Source"));
}
