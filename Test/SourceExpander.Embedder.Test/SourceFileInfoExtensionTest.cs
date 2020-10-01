using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using Xunit;

namespace SourceExpander.Embedder.Test
{
    public class SourceFileInfoExtensionTest
    {
        public static TheoryData ToInitializeStringTestData = new TheoryData<SourceFileInfo, string>
        {
            {
                new SourceFileInfo
                {
                    FileName = "Foo.cs",
                    Usings = new List<string>{ "using System;","using System.Diagnostics;","using static System.Console;" },
                    TypeNames = new List<string>{ "Test.F.N" },
                    Dependencies = new List<string>{ "Test.Put.Nested","Test.Put" },
                    CodeBody = @"namespace Test.F
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
                },
                @"new SourceFileInfo{ FileName=@""Foo.cs"",TypeNames=new string[]{@""Test.F.N""},Usings=new string[]{@""using System;"",@""using System.Diagnostics;"",@""using static System.Console;""},Dependencies=new string[]{@""Test.Put.Nested"",@""Test.Put""},CodeBody=@""namespace Test.F
{
    class N
    {
        public static void WriteN()
        {
            Console.Write(NumType.Zero);
            Write(""""N"""");
            Trace.Write(""""N"""");
            Put.Nested.Write(""""N"""");
        }
    }
}"" }"
            },
        };

        [Theory]
        [MemberData(nameof(ToInitializeStringTestData))]
        public void ToInitializeStringTest(SourceFileInfo info, string expected)
        {
            info.ToInitializeString().Should().Be(expected);
        }
    }
}
