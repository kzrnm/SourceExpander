using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using SourceExpander;

class EntryPoint
{
    static async Task Main()
    {
        var stopwatch = Stopwatch.StartNew();
        var expandTask = Task.Run(Expand);

        Program.Main();
        await expandTask;
        Console.WriteLine($"Finish Expand: {stopwatch.ElapsedMilliseconds} ms");
    }
    static void Expand() => Expander.Expand(inputFilePath: GetProgramPath(), expandMethod: ExpandMethod.Strict);
    static string GetProgramPath() => Path.Combine(Path.GetDirectoryName(MyPath()), "Program.cs");
    static string MyPath([CallerFilePath] string inputFilePath = null) => inputFilePath;
}
