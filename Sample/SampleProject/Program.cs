using System;
using SampleLibrary;


class Program
{
    static void Main()
    {
        Console.WriteLine(typeof(SourceExpander.Expanded.SourceCode));
        Console.WriteLine(SourceExpander.Expanded.ExpandedContainer.Files);
        Put2.Write();
    }
}
