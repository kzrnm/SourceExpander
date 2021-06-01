using System;
using SampleLibrary;

namespace SampleApp
{
    class Program
    {
        static void Main()
        {
            var uf = new UnionFind(3);
            uf.Merge(1, 2);
            Console.WriteLine(uf.Leader(2));
        }
    }
}
