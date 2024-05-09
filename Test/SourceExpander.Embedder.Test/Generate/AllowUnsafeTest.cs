using System.Collections.Immutable;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace SourceExpander.Generate
{
    public class AllowUnsafeTest : EmbedderGeneratorTestBase
    {
        [Fact]
        public async Task AllowButNotUsed()
        {
            const string embeddedSourceCode = "[{\"CodeBody\":\"namespace Test.F{class N{public static void WriteN(){Console.Write(NumType.Zero);Write(\\\"N\\\");Trace.Write(\\\"N\\\");Put.Nested.Write(\\\"N\\\");}}}\",\"Dependencies\":[\"TestProject>F/NumType.cs\",\"TestProject>Put.cs\"],\"FileName\":\"TestProject>F/N.cs\",\"TypeNames\":[\"Test.F.N\"],\"Usings\":[\"using System;\",\"using System.Diagnostics;\",\"using static System.Console;\"]},{\"CodeBody\":\"namespace Test.F{public enum NumType{Zero,Pos,Neg,}}\",\"Dependencies\":[],\"FileName\":\"TestProject>F/NumType.cs\",\"TypeNames\":[\"Test.F.NumType\"],\"Usings\":[]},{\"CodeBody\":\"namespace Test.I{public record IntRecord(int n);[System.Diagnostics.DebuggerDisplay(\\\"TEST\\\")]class D<T>:IComparer<T>{public int Compare(T x,T y)=>throw new NotImplementedException();[System.Diagnostics.Conditional(\\\"TEST\\\")]public static void WriteType(){Console.Write(typeof(T).FullName);Trace.Write(typeof(T).FullName);Put.Nested.Write(typeof(T).FullName);}}}\",\"Dependencies\":[\"TestProject>Put.cs\"],\"FileName\":\"TestProject>I/D.cs\",\"TypeNames\":[\"Test.I.D<T>\",\"Test.I.IntRecord\"],\"Usings\":[\"using System;\",\"using System.Collections.Generic;\",\"using System.Diagnostics;\"]},{\"CodeBody\":\"namespace Test{static class Put{public class Nested{public static void Write(string v){Debug.WriteLine(v);}}}}\",\"Dependencies\":[],\"FileName\":\"TestProject>Put.cs\",\"TypeNames\":[\"Test.Put\",\"Test.Put.Nested\"],\"Usings\":[\"using System.Diagnostics;\"]}]";
            var embeddedNamespaces = ImmutableArray.Create("Test,Test.F,Test.I");
            var embeddedFiles
                = ImmutableArray.Create(
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
                        """namespace Test.I{public record IntRecord(int n);[System.Diagnostics.DebuggerDisplay("TEST")]class D<T>:IComparer<T>{public int Compare(T x,T y)=>throw new NotImplementedException();[System.Diagnostics.Conditional("TEST")]public static void WriteType(){Console.Write(typeof(T).FullName);Trace.Write(typeof(T).FullName);Put.Nested.Write(typeof(T).FullName);}}}"""
                    ), new SourceFileInfo
                    (
                        "TestProject>Put.cs",
                        ["Test.Put", "Test.Put.Nested"],
                        ["using System.Diagnostics;"],
                        ImmutableArray<string>.Empty,
                        "namespace Test{static class Put{public class Nested{public static void Write(string v){Debug.WriteLine(v);}}}}"
                    ));

            var test = new Test
            {
                CompilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true),
                TestState =
                {
                    AdditionalFiles =
                    {
                        enableMinifyJson,
                    },
                    Sources = {
                        (
                            "/home/source/Put.cs",
                            """using System.Diagnostics;namespace Test{static class Put{public class Nested{ public static void Write(string v){Debug.WriteLine(v);}}}}"""
                        ),
                        (
                            "/home/source/I/D.cs",
                            """
        using System.Diagnostics;
        using System; // used 
        using System.Threading.Tasks;// unused
        using System.Collections.Generic;
        namespace Test.I
        {
            using System.Collections;
            public record IntRecord(int n);
            [System.Diagnostics.DebuggerDisplay("TEST")]
            class D<T> : IComparer<T>
            {
                public int Compare(T x,T y) => throw new NotImplementedException();
                [System.Diagnostics.Conditional("TEST")]
                public static void WriteType()
                {
                    Console.Write(typeof(T).FullName);
                    Trace.Write(typeof(T).FullName);
                    Put.Nested.Write(typeof(T).FullName);
                }
            }
        }
        """
                        ),
                        (
                            "/home/source/F/N.cs",
                            """
        using System;
        using System.Diagnostics;
        using static System.Console;

        namespace Test.F
        {
            class N
            {
                public static void WriteN()
                {
                    Console.Write(NumType.Zero);
                    Write("N");
                    Trace.Write("N");
                    Put.Nested.Write("N");
                }
            }
        }
        """
                        ),
                        (
                            "/home/source/F/NumType.cs",
                            """
        namespace Test.F
        {
            public enum NumType
            {
                Zero,
                Pos,
                Neg,
            }
        }
        """
                        ),
                    },
                    ExpectedDiagnostics =
                    {
                        new DiagnosticResult("EMBED0009", DiagnosticSeverity.Info).WithSpan("/home/source/F/N.cs", 3, 1, 3, 29),
                    },
                    GeneratedSources =
                    {
                        (typeof(EmbedderGenerator), "EmbeddedSourceCode.Metadata.cs",
                        EnvironmentUtil.JoinByStringBuilder("using System.Reflection;",
                        "[assembly: AssemblyMetadataAttribute(\"SourceExpander.EmbeddedAllowUnsafe\",\"true\")]",
                        $"[assembly: AssemblyMetadataAttribute(\"SourceExpander.EmbedderVersion\",\"{EmbedderVersion}\")]",
                        $"[assembly: AssemblyMetadataAttribute(\"SourceExpander.EmbeddedLanguageVersion\",\"{EmbeddedLanguageVersion}\")]",
                        $"[assembly: AssemblyMetadataAttribute(\"SourceExpander.EmbeddedNamespaces\",\"{string.Join(",", embeddedNamespaces)}\")]",
                        $"[assembly: AssemblyMetadataAttribute(\"SourceExpander.EmbeddedSourceCode\",{embeddedSourceCode.ToLiteral()})]")
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

        [Fact]
        public async Task UseUnsafe()
        {
            const string embeddedSourceCode = "[{\"CodeBody\":\"public unsafe class C{public static void U(){}}\",\"Dependencies\":[],\"FileName\":\"TestProject>C.cs\",\"TypeNames\":[\"C\"],\"Usings\":[],\"Unsafe\":true},{\"CodeBody\":\"public class C2{public static void Call()=>C.U();}\",\"Dependencies\":[\"TestProject>C.cs\"],\"FileName\":\"TestProject>C2.cs\",\"TypeNames\":[\"C2\"],\"Usings\":[]},{\"CodeBody\":\"public class M{public unsafe static void U(){}}\",\"Dependencies\":[],\"FileName\":\"TestProject>M.cs\",\"TypeNames\":[\"M\"],\"Usings\":[],\"Unsafe\":true},{\"CodeBody\":\"public class M2{public static void Call()=>M.U();}\",\"Dependencies\":[\"TestProject>M.cs\"],\"FileName\":\"TestProject>M2.cs\",\"TypeNames\":[\"M2\"],\"Usings\":[]},{\"CodeBody\":\"public class MC{public static void Call()=>M2.Call();}\",\"Dependencies\":[\"TestProject>M2.cs\"],\"FileName\":\"TestProject>MC.cs\",\"TypeNames\":[\"MC\"],\"Usings\":[]}]";
            var embeddedNamespaces = ImmutableArray<string>.Empty;
            var embeddedFiles
                = ImmutableArray.Create<SourceFileInfo>(
                    new(
                        "TestProject>C.cs",
                        ["C"],
                        [],
                        [],
                        """public unsafe class C{public static void U(){}}""",
                        @unsafe: true
                    ),
                    new(
                        "TestProject>M.cs",
                        ["M"],
                        [],
                        [],
                        """public class M{public unsafe static void U(){}}""",
                        @unsafe: true
                    ),
                    new(
                        "TestProject>C2.cs",
                        ["C2"],
                        [],
                        ["TestProject>C.cs"],
                        """public class C2{public static void Call()=>C.U();}"""
                    ),
                    new(
                        "TestProject>M2.cs",
                        ["M2"],
                        [],
                        ["TestProject>M.cs"],
                        """public class M2{public static void Call()=>M.U();}"""
                    ),
                    new(
                        "TestProject>MC.cs",
                        ["MC"],
                        [],
                        ["TestProject>M2.cs"],
                        """public class MC{public static void Call()=>M2.Call();}"""
                    )
                    );

            var test = new Test
            {
                CompilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true),
                TestState =
                {
                    AdditionalFiles =
                    {
                        enableMinifyJson,
                    },
                    Sources = {
                        (
                            "/home/source/C.cs",
                            """public unsafe class C{public static void U(){}}"""
                        ),
                        (
                            "/home/source/M.cs",
                            """public class M{public unsafe static void U(){}}"""
                        ),
                        (
                            "/home/source/C2.cs",
                            """public class C2{public static void Call()=>C.U();}"""
                        ),
                        (
                            "/home/source/M2.cs",
                            """public class M2{public static void Call()=>M.U();}"""
                        ),
                        (
                            "/home/source/MC.cs",
                            """public class MC{public static void Call()=>M2.Call();}"""
                        ),
                    },
                    GeneratedSources =
                    {
                        (typeof(EmbedderGenerator), "EmbeddedSourceCode.Metadata.cs",
                        EnvironmentUtil.JoinByStringBuilder("using System.Reflection;",
                        "[assembly: AssemblyMetadataAttribute(\"SourceExpander.EmbeddedAllowUnsafe\",\"true\")]",
                        $"[assembly: AssemblyMetadataAttribute(\"SourceExpander.EmbedderVersion\",\"{EmbedderVersion}\")]",
                        $"[assembly: AssemblyMetadataAttribute(\"SourceExpander.EmbeddedLanguageVersion\",\"{EmbeddedLanguageVersion}\")]",
                        $"[assembly: AssemblyMetadataAttribute(\"SourceExpander.EmbeddedNamespaces\",\"{string.Join(",", embeddedNamespaces)}\")]",
                        $"[assembly: AssemblyMetadataAttribute(\"SourceExpander.EmbeddedSourceCode\",{embeddedSourceCode.ToLiteral()})]")
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
