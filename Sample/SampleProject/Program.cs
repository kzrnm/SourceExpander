using System;
using SampleLibrary;

class Program2
{
    static void Main()
    {
        SourceExpander.Expander.Expand(ignoreAnyError: false);
        Put2.Write();
        Console.WriteLine("fofao");
        Console.WriteLine(SampleLibrary.Bit.ExtractLowestSetBit(13));
    }
}
