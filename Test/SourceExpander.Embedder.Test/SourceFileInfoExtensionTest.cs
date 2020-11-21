using System;
using System.Collections.Generic;
using FluentAssertions;
using Newtonsoft.Json;
using Xunit;

namespace SourceExpander.Embedder.Test
{
    public class SourceFileInfoExtensionTest
    {
        public static TheoryData ToJsonTestData = new TheoryData<SourceFileInfo[]>
        {
            {
                new SourceFileInfo[]
                {
                    new SourceFileInfo
                    (
                        "TestAssembly>F/N.cs",
                        new string[] { "Test.F.N" },
                        new string[] { "using System;", "using System.Diagnostics;", "using static System.Console;" },
                        new string[] { "TestAssembly>F/NumType.cs", "TestAssembly>Put.cs" },
                        "namespace Test.F { class N { public static void WriteN() { Console.Write(NumType.Zero); Write(\"N\"); Trace.Write(\"N\"); Put.Nested.Write(\"N\"); } } }"
                    ), new SourceFileInfo
                    (
                        "TestAssembly>F/NumType.cs",
                        new string[] { "Test.F.NumType" },
                        Array.Empty<string>(),
                        Array.Empty<string>(),
                        "namespace Test.F { public enum NumType { Zero, Pos, Neg, } }"
                    ), new SourceFileInfo
                    (
                        "TestAssembly>I/D.cs",
                        new string[] { "Test.I.D<T>" },
                        new string[] { "using System.Diagnostics;", "using System;", "using System.Collections.Generic;" },
                        new string[] { "TestAssembly>Put.cs" },
                        "namespace Test.I { class D<T> : IComparer<T> { public int Compare(T x, T y) => throw new NotImplementedException(); public static void WriteType() { Console.Write(typeof(T).FullName); Trace.Write(typeof(T).FullName); Put.Nested.Write(typeof(T).FullName); } } }"
                    ), new SourceFileInfo
                    (
                        "TestAssembly>Put.cs",
                        new string[] { "Test.Put", "Test.Put.Nested" },
                        new string[] { "using System.Diagnostics;" },
                        Array.Empty<string>(),
                        "namespace Test{static class Put{public class Nested{ public static void Write(string v){Debug.WriteLine(v);}}}}"
                    )
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
