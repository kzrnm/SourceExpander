using System.Collections.Immutable;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace SourceExpander.Embedder.Generate.Test
{
    public class SkipTypeTest : EmbedderGeneratorTestBase
    {
        [Fact]
        public async Task Generate()
        {
            var embeddedFiles = ImmutableArray.Create(
                   new SourceFileInfo
                   (
                       "TestProject>F/NumType.cs",
                       new string[] { "Test.F.NumType" },
                       ImmutableArray.Create<string>(),
                       ImmutableArray.Create<string>(),
                       "namespace Test.F{public enum NumType{Zero,Pos,Neg,}}"
                   ), new SourceFileInfo
                   (
                       "TestProject>I/D.cs",
                       new string[] { "Test.I.IntRecord", "Test.I.D<T>" },
                       new string[] { "using System.Diagnostics;", "using System;", "using System.Collections.Generic;" },
                       new string[] { "TestProject>Put.cs" },
                       @"namespace Test.I{public record IntRecord(int n);[System.Diagnostics.DebuggerDisplay(""TEST"")]class D<T>:IComparer<T>{public int Compare(T x,T y)=>throw new NotImplementedException();[System.Diagnostics.Conditional(""TEST"")]public static void WriteType(){Console.Write(typeof(T).FullName);Trace.Write(typeof(T).FullName);Put.Nested.Write(typeof(T).FullName);}}}"
                   ), new SourceFileInfo
                   (
                       "TestProject>Put.cs",
                       new string[] { "Test.Put", "Test.Put.Nested" },
                       new string[] { "using System.Diagnostics;" },
                       ImmutableArray.Create<string>(),
                       "namespace Test{static class Put{public class Nested{public static void Write(string v){Debug.WriteLine(v);}}}}"
                   ));

            const string embeddedSourceCode = "[{\"CodeBody\":\"namespace Test.F{public enum NumType{Zero,Pos,Neg,}}\",\"Dependencies\":[],\"FileName\":\"TestProject>F\\/NumType.cs\",\"TypeNames\":[\"Test.F.NumType\"],\"Usings\":[]},{\"CodeBody\":\"namespace Test.I{public record IntRecord(int n);[System.Diagnostics.DebuggerDisplay(\\\"TEST\\\")]class D<T>:IComparer<T>{public int Compare(T x,T y)=>throw new NotImplementedException();[System.Diagnostics.Conditional(\\\"TEST\\\")]public static void WriteType(){Console.Write(typeof(T).FullName);Trace.Write(typeof(T).FullName);Put.Nested.Write(typeof(T).FullName);}}}\",\"Dependencies\":[\"TestProject>Put.cs\"],\"FileName\":\"TestProject>I\\/D.cs\",\"TypeNames\":[\"Test.I.D<T>\",\"Test.I.IntRecord\"],\"Usings\":[\"using System;\",\"using System.Collections.Generic;\",\"using System.Diagnostics;\"]},{\"CodeBody\":\"namespace Test{static class Put{public class Nested{public static void Write(string v){Debug.WriteLine(v);}}}}\",\"Dependencies\":[],\"FileName\":\"TestProject>Put.cs\",\"TypeNames\":[\"Test.Put\",\"Test.Put.Nested\"],\"Usings\":[\"using System.Diagnostics;\"]}]";
            var test = new Test
            {
                TestState =
                {
                    AdditionalFiles =
                    {
                        enableMinifyJson,
                    },
                    Sources = {
                        (
                        "/home/source/Put.cs",
                        @"using System.Diagnostics;namespace Test{static class Put{public class Nested{ public static void Write(string v){Debug.WriteLine(v);}}}}"
                        ),
                        (
                        "/home/source/I/D.cs",
                        @"using System.Diagnostics;
    using System; // used 
    using System.Threading.Tasks;// unused
    using System.Collections.Generic;
    namespace Test.I
    {
        using System.Collections;
        public record IntRecord(int n);
        [System.Diagnostics.DebuggerDisplay(""TEST"")]
        class D<T> : IComparer<T>
        {
            public int Compare(T x,T y) => throw new NotImplementedException();
            [System.Diagnostics.Conditional(""TEST"")]
            public static void WriteType()
            {
                Console.Write(typeof(T).FullName);
                Trace.Write(typeof(T).FullName);
                Put.Nested.Write(typeof(T).FullName);
            }
        }
    }"
                        ),
                        (
                        "/home/source/F/N.cs",
                        @"using System;
    using System.Diagnostics;
    using static System.Console;
using SourceExpander;

namespace Test.F
{
    [SourceExpander.NotEmbeddingSource]
    class N
    {
        /// <summary>
        /// XML Document
        /// </summary>
        public static void WriteN()
        {
            Console.Write(NumType.Zero);
            Write(""N"");
            Trace.Write(""N"");
            Put.Nested.Write(""N"");
        }
    }

    [NotEmbeddingSource]
    struct R
    {
        /// <summary>
        /// XML Document
        /// </summary>
        public static void WriteN()
        {
            Console.Write(NumType.Zero);
            Write(""N"");
            Trace.Write(""N"");
            Put.Nested.Write(""N"");
        }
    }
}"
                        ),
                        (
                        "/home/source/F/NumType.cs",
       @"
    namespace Test.F
    {
        public enum NumType
        {
            Zero,
            Pos,
            Neg,
        }
    }"
                        ),
                    },
                    GeneratedSources =
                    {
                        (typeof(EmbedderGenerator), "EmbeddedSourceCode.Metadata.cs", @$"using System.Reflection;
[assembly: AssemblyMetadataAttribute(""SourceExpander.EmbedderVersion"",""{EmbedderVersion}"")]
[assembly: AssemblyMetadataAttribute(""SourceExpander.EmbeddedLanguageVersion"",""{EmbeddedLanguageVersion}"")]
[assembly: AssemblyMetadataAttribute(""SourceExpander.EmbeddedSourceCode"",{embeddedSourceCode.ToLiteral()})]
"),
                    },
                    ExpectedDiagnostics = {
                        DiagnosticResult.CompilerError("CS0234").WithSpan("/home/source/F/N.cs", 8, 21, 8, 39).WithArguments("NotEmbeddingSource", "SourceExpander"),
                        DiagnosticResult.CompilerError("CS0234").WithSpan("/home/source/F/N.cs", 8, 21, 8, 39).WithArguments("NotEmbeddingSourceAttribute", "SourceExpander"),
                        DiagnosticResult.CompilerError("CS0246").WithSpan("/home/source/F/N.cs", 23, 6, 23, 24).WithArguments("NotEmbeddingSource"),
                        DiagnosticResult.CompilerError("CS0246").WithSpan("/home/source/F/N.cs", 23, 6, 23, 24).WithArguments("NotEmbeddingSourceAttribute"),
                    },
                }
            };
            await test.RunAsync();
            Newtonsoft.Json.JsonConvert.DeserializeObject<SourceFileInfo[]>(embeddedSourceCode)
                .Should()
                .BeEquivalentTo(embeddedFiles);
            System.Text.Json.JsonSerializer.Deserialize<SourceFileInfo[]>(embeddedSourceCode)
                .Should()
                .BeEquivalentTo(embeddedFiles);
        }
    }
}

