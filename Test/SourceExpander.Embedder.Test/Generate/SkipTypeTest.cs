using System.Collections.Immutable;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace SourceExpander.Embedder.Generate.Test
{
    public class SkipTypeTest : EmbeddingGeneratorTestBase
    {
        [Fact(Skip = "https://github.com/dotnet/roslyn-sdk/issues/762")]
        public async Task Generate()
        {
            var embeddedFiles = ImmutableArray.Create(
                   new SourceFileInfo
                   (
                       "TestAssembly>F/NumType.cs",
                       new string[] { "Test.F.NumType" },
                       ImmutableArray.Create<string>(),
                       ImmutableArray.Create<string>(),
                       "namespace Test.F{public enum NumType{Zero,Pos,Neg,}}"
                   ), new SourceFileInfo
                   (
                       "TestAssembly>I/D.cs",
                       new string[] { "Test.I.IntRecord", "Test.I.D<T>" },
                       new string[] { "using System.Diagnostics;", "using System;", "using System.Collections.Generic;" },
                       new string[] { "TestAssembly>Put.cs" },
                       @"namespace Test.I{public record IntRecord(int n);[System.Diagnostics.DebuggerDisplay(""TEST"")]class D<T>:IComparer<T>{public int Compare(T x,T y)=>throw new NotImplementedException();[System.Diagnostics.Conditional(""TEST"")]public static void WriteType(){Console.Write(typeof(T).FullName);Trace.Write(typeof(T).FullName);Put.Nested.Write(typeof(T).FullName);}}}"
                   ), new SourceFileInfo
                   (
                       "TestAssembly>Put.cs",
                       new string[] { "Test.Put", "Test.Put.Nested" },
                       new string[] { "using System.Diagnostics;" },
                       ImmutableArray.Create<string>(),
                       "namespace Test{static class Put{public class Nested{public static void Write(string v){Debug.WriteLine(v);}}}}"
                   ));

            const string embeddedSourceCode = "";
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
[assembly: AssemblyMetadataAttribute(""SourceExpander.EmbeddedSourceCode.GZipBase32768"",""{embeddedSourceCode}"")]
"),
                    }
                }
            };
            await test.RunAsync();
            Newtonsoft.Json.JsonConvert.DeserializeObject<SourceFileInfo[]>(
                SourceFileInfoUtil.FromGZipBase32768(embeddedSourceCode))
                .Should()
                .BeEquivalentTo(embeddedFiles);
            System.Text.Json.JsonSerializer.Deserialize<SourceFileInfo[]>(
                SourceFileInfoUtil.FromGZipBase32768(embeddedSourceCode))
                .Should()
                .BeEquivalentTo(embeddedFiles);
        }
    }
}
