using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace SourceExpander.Embedder
{
    static class TestUtil
    {
        public static readonly MetadataReference[] defaultMetadatas = GeneratorUtil.GetDefaulMetadatas().ToArray();
        public static readonly MetadataReference expanderCoreReference = MetadataReference.CreateFromFile(typeof(SourceFileInfo).Assembly.Location);


        public static readonly int TestSyntaxesCount = GetTestSyntaxes().Count();
        public static IEnumerable<SyntaxTree> GetTestSyntaxes()
        {
            yield return CSharpSyntaxTree.ParseText(
                @"using System.Diagnostics;namespace Test{static class Put{public class Nested{ public static void Write(string v){Debug.WriteLine(v);}}}}",
                path: "/home/source/Put.cs");
            yield return CSharpSyntaxTree.ParseText(
                @"using System.Diagnostics;
using System;
using System.Threading.Tasks;// unused
using System.Collections.Generic;
namespace Test.I
{
    using System.Collections;
    public record IntRecord(int n);
    class D<T> : IComparer<T>
    {
        public int Compare(T x,T y) => throw new NotImplementedException();
        public static void WriteType()
        {
            Console.Write(typeof(T).FullName);
            Trace.Write(typeof(T).FullName);
            Put.Nested.Write(typeof(T).FullName);
        }
    }
}",
                path: "/home/source/I/D.cs");
            yield return CSharpSyntaxTree.ParseText(
               @"using System;
using System.Diagnostics;
using static System.Console;

namespace Test.F
{
    class N
    {
        public static void WriteN()
        {
            Console.Write(NumType.Zero);
            Write(""N"");
            Trace.Write(""N"");
            Put.Nested.Write(""N"");
        }
    }
}",
                path: "/home/source/F/N.cs");
            yield return CSharpSyntaxTree.ParseText(
   @"
namespace Test.F
{
    public enum NumType
    {
        Zero,
        Pos,
        Neg,
    }
}", path: "/home/source/F/NumType.cs");
        }
    }
}
