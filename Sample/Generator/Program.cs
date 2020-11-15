using SampleLibrary;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
class Program
{
    [Conditional("DEBUG")]
    static void Expand()
    {
#if DEBUG
        static string CurrentPath([CallerFilePath] string path = "") => path;
        var path = CurrentPath();
        var code = Expanded.Expanded.Files[path].Code;
        File.WriteAllText(path.Replace("Program.cs", "Combined.csx"), code);
#endif
    }

    public static void Main()
    {
        Expand();
        Console.WriteLine(42);
        Put2.Write();
    }
}
