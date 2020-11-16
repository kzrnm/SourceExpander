using System;
using System.Collections.Generic;

namespace SourceExpander.Expanders
{
    public static class SourceUtil
    {
        public static SourceFileContainer SourceFiles => new SourceFileContainer(new[]
        {
            new SourceFileInfo
            {
                FileName = "Put.cs",
                TypeNames = new[] { "Test.Put" },
                Usings = new[] { "using System.Diagnostics;" },
                Dependencies = Array.Empty<string>(),
                CodeBody = @"namespace Test{static class Put{public static void Write(string v){Debug.WriteLine(v);}}}"
            },
            new SourceFileInfo
            {
                FileName = "I/D.cs",
                TypeNames = new[] { "Test.I.D<T>" },
                Usings = new[] { "using System.Diagnostics;", "using System;" },
                Dependencies = new[] { "Put.cs" },
                CodeBody = @"namespace Test.I
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
}" },
            new SourceFileInfo
            {
                FileName = "F/N.cs",
                TypeNames = new[] { "Test.F.N" },
                Usings = new[] { "using System.Diagnostics;", "using System;" },
                Dependencies = new[] { "Put.cs" },
                CodeBody = @"namespace Test.F
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
}" }
            });
    }
}
