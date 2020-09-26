using System.IO;
using FluentAssertions;
using SourceExpander.Expanders.Utils;
using Xunit;

namespace SourceExpander.Expanders
{
    public class CompilationExpanderTest
    {
        [Fact]
        public void ExpandTest()
        {
            const string origCode = @"using System;
using System.Text;
using Test.I;
using Test.F;

namespace ExpanderTest
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine(42);
            D<short>.WriteType();
        }
    }
}
";
            const string expected = @"using System;
using System.Diagnostics;
using Test.I;
namespace ExpanderTest
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine(42);
            D<short>.WriteType();
        }
    }
}
#region Expanded
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
}
namespace Test{static class Put{public static void Write(string v){Debug.WriteLine(v);}}}
#endregion Expanded
";

            using var sr = new StringReader(origCode);
            var expander = new CompilationExpander(origCode, SourceUtil.SourceFiles);

            var lineNum = 0;
            foreach (var (line, expectedLine) in expander.ExpandedLines().ZipAndFill<string>(ExpanderUtil.ToLines(expected)))
            {
                ++lineNum;
                line.Should().Be(expectedLine, "diffrent at line:{0}", lineNum);
            }

            var gotCode = expander.ExpandedString();
            TestUtil.TestCompile(gotCode);
        }
    }
}
