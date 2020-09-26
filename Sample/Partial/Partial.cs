using System;
using System.Threading.Tasks;
using SourceExpander;

partial class Program
{
    static Task expandTask;
    static partial void Expand(string path)
    {
        AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
        expandTask = Task.Run(() => Expander.Expand(inputFilePath: path, expandMethod: ExpandMethod.Strict));
    }

    private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
    {
        expandTask.Wait();
        Console.WriteLine($"Finish Expand: {stopwatch.ElapsedMilliseconds} ms");
    }
}
