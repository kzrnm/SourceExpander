﻿using System.Collections.Immutable;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace SourceExpander.Generate
{
    public class MultiSyntaxTest : EmbedderGeneratorTestBase
    {
        [Fact]
        public async Task Generate()
        {
            var embeddedNamespaces = ImmutableArray.Create("Test,Test.F,Test.I");
            var embeddedFiles = ImmutableArray.Create(
                   new SourceFileInfo
                   (
                       "TestProject>F/N.cs",
                       ["Test.F.N"],
                       ["using System;", "using System.Diagnostics;", "using static System.Console;"],
                       ["TestProject>F/NumType.cs", "TestProject>Put.cs"],
                       "namespace Test.F{class N{public static void WriteN(){Console.Write(NumType.Zero);Write(\"N\");Trace.Write(\"N\");Put.Nested.Write(\"N\");}}}"
                   ), new SourceFileInfo
                   (
                       "TestProject>F/NumType.cs",
                       ["Test.F.NumType"],
                       ImmutableArray<string>.Empty,
                       ImmutableArray<string>.Empty,
                       "namespace Test.F{public enum NumType{Zero,Pos,Neg,}}"
                   ), new SourceFileInfo
                   (
                       "TestProject>I/D.cs",
                       ["Test.I.IntRecord", "Test.I.D<T>"],
                       ["using System.Diagnostics;", "using System;", "using System.Collections.Generic;"],
                       ["TestProject>Put.cs"],
                       @"namespace Test.I{public record IntRecord(int n);[System.Diagnostics.DebuggerDisplay(""TEST"")]class D<T>:IComparer<T>{public int Compare(T x,T y)=>throw new NotImplementedException();[System.Diagnostics.Conditional(""TEST"")]public static void WriteType(){Console.Write(typeof(T).FullName);Trace.Write(typeof(T).FullName);Put.Nested.Write(typeof(T).FullName);}}}"
                   ), new SourceFileInfo
                   (
                       "TestProject>Put.cs",
                       ["Test.Put", "Test.Put.Nested"],
                       ["using System.Diagnostics;"],
                       ImmutableArray<string>.Empty,
                       "namespace Test{static class Put{public class Nested{public static void Write(string v){Debug.WriteLine(v);}}}}"
                   ));
            const string embeddedSourceCode = "[{\"CodeBody\":\"namespace Test.F{class N{public static void WriteN(){Console.Write(NumType.Zero);Write(\\\"N\\\");Trace.Write(\\\"N\\\");Put.Nested.Write(\\\"N\\\");}}}\",\"Dependencies\":[\"TestProject>F/NumType.cs\",\"TestProject>Put.cs\"],\"FileName\":\"TestProject>F/N.cs\",\"TypeNames\":[\"Test.F.N\"],\"Usings\":[\"using System;\",\"using System.Diagnostics;\",\"using static System.Console;\"]},{\"CodeBody\":\"namespace Test.F{public enum NumType{Zero,Pos,Neg,}}\",\"Dependencies\":[],\"FileName\":\"TestProject>F/NumType.cs\",\"TypeNames\":[\"Test.F.NumType\"],\"Usings\":[]},{\"CodeBody\":\"namespace Test.I{public record IntRecord(int n);[System.Diagnostics.DebuggerDisplay(\\\"TEST\\\")]class D<T>:IComparer<T>{public int Compare(T x,T y)=>throw new NotImplementedException();[System.Diagnostics.Conditional(\\\"TEST\\\")]public static void WriteType(){Console.Write(typeof(T).FullName);Trace.Write(typeof(T).FullName);Put.Nested.Write(typeof(T).FullName);}}}\",\"Dependencies\":[\"TestProject>Put.cs\"],\"FileName\":\"TestProject>I/D.cs\",\"TypeNames\":[\"Test.I.D<T>\",\"Test.I.IntRecord\"],\"Usings\":[\"using System;\",\"using System.Collections.Generic;\",\"using System.Diagnostics;\"]},{\"CodeBody\":\"namespace Test{static class Put{public class Nested{public static void Write(string v){Debug.WriteLine(v);}}}}\",\"Dependencies\":[],\"FileName\":\"TestProject>Put.cs\",\"TypeNames\":[\"Test.Put\",\"Test.Put.Nested\"],\"Usings\":[\"using System.Diagnostics;\"]}]";
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
                    ExpectedDiagnostics =
                    {
                        new DiagnosticResult("EMBED0009", DiagnosticSeverity.Info).WithSpan("/home/source/F/N.cs", 3, 9, 3, 37),
                    },
                    GeneratedSources =
                    {
                        (typeof(EmbedderGenerator), "EmbeddedSourceCode.Metadata.cs",$"""
                        // <auto-generated/>
                        #pragma warning disable
                        [assembly: global::System.Reflection.AssemblyMetadataAttribute("SourceExpander.EmbedderVersion","{EmbedderVersion}")]
                        [assembly: global::System.Reflection.AssemblyMetadataAttribute("SourceExpander.EmbeddedLanguageVersion","{EmbeddedLanguageVersion}")]
                        [assembly: global::System.Reflection.AssemblyMetadataAttribute("SourceExpander.EmbeddedNamespaces","{string.Join(",", embeddedNamespaces)}")]
                        [assembly: global::System.Reflection.AssemblyMetadataAttribute("SourceExpander.EmbeddedSourceCode",{embeddedSourceCode.ToLiteral()})]
                        
                        """
                        ),
                    }
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
