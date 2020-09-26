using System;
using System.Collections.Generic;

namespace SourceExpander.Expanders
{
    public static class SourceUtil
    {
        public static SourceFileContainer SourceFiles => new SourceFileContainer(new[]
        {
            new SourceFileInfo(
                "Put.cs",
                new[] { "Test.Put" },
                new[] { "using System.Diagnostics;" },
                Array.Empty<string>(),
                @"namespace Test{static class Put{public static void Write(string v){Debug.WriteLine(v);}}}"),
            new SourceFileInfo(
                "I/D.cs",
                new[] { "Test.I.D<T>" },
                new[] { "using System.Diagnostics;", "using System;" },
                new[] { "Put.cs" },
                @"namespace Test.I
{
    class D<T>
    {
        public static void WriteType()
        {
            Console.Write(typeof(T).FullName);
            Trace.Write(typeof(T).FullName);
            Put.Write(typeof(T).FullName);
        }
    }
}"),
            new SourceFileInfo(
                "F/N.cs",
                new[] { "Test.F.N" },
                new[] { "using System.Diagnostics;", "using System;" },
                new[] { "Put.cs" },
                @"namespace Test.F
{
    class N
    {
        public static void WriteN()
        {
            Console.Write(""N"");
            Trace.Write(""N"");
            Put.Write(""N"");
        }
    }
}")
            });
    }
}
