﻿using System;
using System.Diagnostics;
using System.IO;
using SourceExpander;

class Program
{
    static void Main()
    {
        var sw = Stopwatch.StartNew();
        var path = "./Program2.cs";
        // ExpandFile(path);
        Console.WriteLine(ExpandString(File.ReadAllText(path)));
        Console.Error.WriteLine($"expand time: {sw.ElapsedMilliseconds} ms");
    }

    static void ExpandFile(string path) => Expander.Expand(path, expandMethod: ExpandMethod.Strict);
    static string ExpandString(string code) => Expander.Create(code, expandMethod: ExpandMethod.Strict).ExpandedString();
}
