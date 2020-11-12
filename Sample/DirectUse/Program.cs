using System;
using SampleLibrary;
using SourceExpander;

class Program
{
    static void Main()
    {
        Expander.Expand(expandMethod: ExpandMethod.All);
        Console.WriteLine(42);
        Put.WriteRandom();
    }
}
