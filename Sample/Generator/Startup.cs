using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Linq;
class Startup
{
    static void Main()
    {
        Program.Main();
        var code = Expanded.Expanded.Files.First(kv => kv.Key.EndsWith("Program.cs")).Value.Code;
        File.WriteAllText(CurrentPath().Replace("Startup.cs", "Combined.csx"), code);
    }
    static string CurrentPath([CallerFilePath] string path = "") => path;
}
