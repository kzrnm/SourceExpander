using System;
using FluentAssertions;
using Xunit;

namespace SourceExpander.Embedder.Test
{
    public class SourceFileResolverTest
    {
        [Fact]
        public void ResolveTest()
        {
            var resolver = new SourceFileResolver("TestModule", SourceUtil.SourceFiles);
            var resolved = resolver.Resolve();
            resolved.Should().BeEquivalentTo(new[]
            {
                new SourceFileInfo
                {
                    FileName = "TestModule>Put.cs",
                    TypeNames = new[] { "Test.Put" },
                    Usings = new[] { "using System.Diagnostics;" },
                    Dependencies = Array.Empty<string>(),
                    CodeBody = @"namespace Test{static class Put{public static void Write(string v){Debug.WriteLine(v);}}}"
                },
                new SourceFileInfo
                {
                    FileName = "TestModule>I/D.cs",
                    TypeNames = new[] { "Test.I.D<T>" },
                    Usings = new[] { "using System.Diagnostics;", "using System;" },
                    Dependencies = new[] { "TestModule>Put.cs" },
                    CodeBody = @"namespace Test.I { class D<T> { public static void WriteType() { Console.Write(typeof(T).FullName); Trace.Write(typeof(T).FullName); Put.Write(typeof(T).FullName); } } }"
                },
                new SourceFileInfo
                {
                    FileName = "TestModule>F/N.cs",
                    TypeNames = new[] { "Test.F.N" },
                    Usings = new[] { "using System;", "using System.Diagnostics;" },
                    Dependencies = new[] { "TestModule>Put.cs", "TestModule>F/NumType.cs" },
                    CodeBody = @"namespace Test.F { class N { public static void WriteN() { Console.Write(NumType.Zero); Console.Write(""N""); Trace.Write(""N""); Put.Write(""N""); } } }"
                },
                new SourceFileInfo
                {
                    FileName = "TestModule>F/NumType.cs",
                    TypeNames = new[] { "Test.F.NumType" },
                    Usings = Array.Empty<string>(),
                    Dependencies = Array.Empty<string>(),
                    CodeBody = @"namespace Test.F { public enum NumType { Zero, Pos, Neg, } }"
                },
            });
        }
    }
}
