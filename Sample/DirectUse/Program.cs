using System;
using SampleLibrary;
using SourceExpander;

class Program
{
    static void Main()
    {
        Console.WriteLine(42);
        Put2.Write();
        Expander.Expand(expandMethod: ExpandMethod.All);
    }
}
