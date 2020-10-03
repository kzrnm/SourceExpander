using System;
using System.Collections.Generic;
using System.Text.Json;
using FluentAssertions;
using Newtonsoft.Json;
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


        public static TheoryData ToJsonTestData = new TheoryData<SourceFileInfo[]>
        {
            {
                new SourceFileInfo[]
                {
                    new SourceFileInfo
                    {
                        FileName = "TestAssembly>F/N.cs",
                        TypeNames = new string[] { "Test.F.N" },
                        Usings = new string[] { "using System;", "using System.Diagnostics;", "using static System.Console;" },
                        Dependencies = new string[] { "TestAssembly>F/NumType.cs", "TestAssembly>Put.cs" },
                        CodeBody = "namespace Test.F { class N { public static void WriteN() { Console.Write(NumType.Zero); Write(\"N\"); Trace.Write(\"N\"); Put.Nested.Write(\"N\"); } } }",
                    }, new SourceFileInfo
                    {
                        FileName = "TestAssembly>F/NumType.cs",
                        TypeNames = new string[] { "Test.F.NumType" },
                        Usings = Array.Empty<string>(),
                        Dependencies = Array.Empty<string>(),
                        CodeBody = "namespace Test.F { public enum NumType { Zero, Pos, Neg, } }",
                    }, new SourceFileInfo
                    {
                        FileName = "TestAssembly>I/D.cs",
                        TypeNames = new string[] { "Test.I.D<T>" },
                        Usings = new string[] { "using System.Diagnostics;", "using System;", "using System.Collections.Generic;" },
                        Dependencies = new string[] { "TestAssembly>Put.cs" },
                        CodeBody = "namespace Test.I { class D<T> : IComparer<T> { public int Compare(T x, T y) => throw new NotImplementedException(); public static void WriteType() { Console.Write(typeof(T).FullName); Trace.Write(typeof(T).FullName); Put.Nested.Write(typeof(T).FullName); } } }",
                    }, new SourceFileInfo
                    {
                        FileName = "TestAssembly>Put.cs",
                        TypeNames = new string[] { "Test.Put", "Test.Put.Nested" },
                        Usings = new string[] { "using System.Diagnostics;" },
                        Dependencies = Array.Empty<string>(),
                        CodeBody = "namespace Test{static class Put{public class Nested{ public static void Write(string v){Debug.WriteLine(v);}}}}",
                    }
                }
            },
        };

        [Theory]
        [MemberData(nameof(ToJsonTestData))]
        public void ToJsonTest(SourceFileInfo[] infos)
        {
            var json = infos.ToJson();
            JsonConvert.DeserializeObject<SourceFileInfo[]>(json).Should().BeEquivalentTo(infos);
        }
    }
}
