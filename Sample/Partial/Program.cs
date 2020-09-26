using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

partial class Program
{
    static Stopwatch stopwatch;
    static partial void Expand([CallerFilePath] string path = null);
    public static void Main()
    {
        stopwatch = Stopwatch.StartNew();
        Console.WriteLine($"Start: {stopwatch.ElapsedMilliseconds} ms");
        Expand();
        Console.WriteLine($"Start Expand: {stopwatch.ElapsedMilliseconds} ms");
    }
}
