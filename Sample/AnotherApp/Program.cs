using System;
using System.Diagnostics;
using System.IO;
using SourceExpander;

class Program
{
    static void Main()
    {
        var sw = Stopwatch.StartNew();
        var path = "./Program2.cs";
        var container = GlobalSourceFileContainer.Instance;
        // ExpandFile(path);
        Trace.WriteLine(typeof(SampleLibrary.Put)); // Load Assembly
        Console.Error.WriteLine(container.Count);
        Console.WriteLine(ExpandString(File.ReadAllText(path)));
        Console.Error.WriteLine($"expand time: {sw.ElapsedMilliseconds} ms");
    }

    static void ExpandFile(string path) => Expander.Expand(path, expandMethod: ExpandMethod.Strict);
    static string ExpandString(string code) => Expander.Create(code, expandMethod: ExpandMethod.Strict).ExpandedString();
}
