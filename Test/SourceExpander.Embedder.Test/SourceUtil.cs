using System.Collections.Generic;

namespace SourceExpander.Embedder.Test
{
    public static class SourceUtil
    {
        public static IEnumerable<SourceFileInfoRaw> SourceFiles => new[]
        {
            new SourceFileInfoRaw(
                "Put.cs",
                @"using System.Diagnostics;namespace Test{static class Put{public static void Write(string v){Debug.WriteLine(v);}}}"
            ),
            new SourceFileInfoRaw(
                "I/D.cs",
                @"using System.Diagnostics;
using System;
namespace Test.I
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
            new SourceFileInfoRaw(
                "F/N.cs",
               @"using System;
using System.Diagnostics;

namespace Test.F
{
    class N
    {
        public static void WriteN()
        {
            Console.Write(NumType.Zero);
            Console.Write(""N"");
            Trace.Write(""N"");
            Put.Write(""N"");
        }
    }
}"),
            new SourceFileInfoRaw(
                "F/NumType.cs",
               @"
namespace Test.F
{
    public enum NumType
    {
        Zero,
        Pos,
        Neg,
    }
}")
            };
    }
}
