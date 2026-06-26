using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;

namespace SourceExpander.Generate;

public class NotEmbeddingSourceTest : EmbedderGeneratorTestBase
{
    [Test]
    public async Task Generate(CancellationToken cancellationToken)
    {
        var embeddedNamespaces = ImmutableArray.Create("Test,Test.F,Test.I");
        var embeddedFiles = ImmutableArray.Create(
               new SourceFileInfo
               (
                   "TestProject>F/NumType.cs",
                   ["Test.F.NumType"],
                   ImmutableArray<string>.Empty,
                   ImmutableArray<string>.Empty,
                   // lang=C#
                   "namespace Test.F{public enum NumType{Zero,Pos,Neg,}}"
               ), new SourceFileInfo
               (
                   "TestProject>I/D.cs",
                   ["Test.I.IntRecord", "Test.I.D<T>"],
                   // lang=C#
                   ["using System.Diagnostics;", "using System;", "using System.Collections.Generic;"],
                   ["TestProject>Put.cs"],
                   // lang=C#
                   """namespace Test.I{public record IntRecord(int n);[System.Diagnostics.DebuggerDisplay("TEST")]class D<T>:IComparer<T>{public int Compare(T x,T y)=>throw new NotImplementedException();[System.Diagnostics.Conditional("TEST")]public static void WriteType(){Console.Write(typeof(T).FullName);Trace.Write(typeof(T).FullName);Put.Nested.Write(typeof(T).FullName);}void OuterMethod(){}}}"""
               ), new SourceFileInfo
               (
                   "TestProject>Put.cs",
                   ["Test.Put", "Test.Put.Nested"],
                   // lang=C#
                   ["using System.Diagnostics;"],
                   ImmutableArray<string>.Empty,
                   // lang=C#
                   "namespace Test{static class Put{public class Nested{public static void Write(string v){Debug.WriteLine(v);}}}}"
               ));

        const string embeddedSourceCode = "[{\"CodeBody\":\"namespace Test.F{public enum NumType{Zero,Pos,Neg,}}\",\"Dependencies\":[],\"FileName\":\"TestProject>F/NumType.cs\",\"TypeNames\":[\"Test.F.NumType\"],\"Usings\":[]},{\"CodeBody\":\"namespace Test.I{public record IntRecord(int n);[System.Diagnostics.DebuggerDisplay(\\\"TEST\\\")]class D<T>:IComparer<T>{public int Compare(T x,T y)=>throw new NotImplementedException();[System.Diagnostics.Conditional(\\\"TEST\\\")]public static void WriteType(){Console.Write(typeof(T).FullName);Trace.Write(typeof(T).FullName);Put.Nested.Write(typeof(T).FullName);}void OuterMethod(){}}}\",\"Dependencies\":[\"TestProject>Put.cs\"],\"FileName\":\"TestProject>I/D.cs\",\"TypeNames\":[\"Test.I.D<T>\",\"Test.I.IntRecord\"],\"Usings\":[\"using System;\",\"using System.Collections.Generic;\",\"using System.Diagnostics;\"]},{\"CodeBody\":\"namespace Test{static class Put{public class Nested{public static void Write(string v){Debug.WriteLine(v);}}}}\",\"Dependencies\":[],\"FileName\":\"TestProject>Put.cs\",\"TypeNames\":[\"Test.Put\",\"Test.Put.Nested\"],\"Usings\":[\"using System.Diagnostics;\"]}]";
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
                   // lang=C#
                    """using System.Diagnostics;namespace Test{static class Put{public class Nested{ public static void Write(string v){Debug.WriteLine(v);}}}}"""
                    ),
                    (
                    "/home/source/I/D.cs",
                   // lang=C#
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
        
        void OuterMethod()
        {
            [SourceExpander.NotEmbeddingSource]
            static void InnerMethod() { }
        }
    }
}
"""
                    ),
                    (
                    "/home/source/F/N.cs",
                   // lang=C#
                    """
using System;
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
            Write("N");
            Trace.Write("N");
            Put.Nested.Write("N");
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
                   // lang=C#
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
                },
                ExpectedDiagnostics = {
                    new DiagnosticResult("EMBED0009", DiagnosticSeverity.Info).WithSpan("/home/source/F/N.cs", 3, 1, 3, 29),
                },
            }
        };
        await test.RunAsync(cancellationToken);
        await Newtonsoft.Json.JsonConvert.DeserializeObject<SourceFileInfo[]>(embeddedSourceCode)
            .Should().BeEquivalentTo(embeddedFiles);
        await System.Text.Json.JsonSerializer.Deserialize<SourceFileInfo[]>(embeddedSourceCode)
            .Should().BeEquivalentTo(embeddedFiles);
    }

    [Test]
    public async Task PropertyAccessor(CancellationToken cancellationToken)
    {
        var embeddedNamespaces = ImmutableArray<string>.Empty;
        var embeddedFiles = ImmutableArray.Create(
             new SourceFileInfo
             (
                 "TestProject>Program.cs",
                 ["Program"],
                 // lang=C#
                 ImmutableArray<string>.Empty,
                 ImmutableArray<string>.Empty,
                 // lang=C#
                 "class Program{public int Value{get;set;}}"
             ));
        const string embeddedSourceCode = "[{\"CodeBody\":\"class Program{public int Value{get;set;}}\",\"Dependencies\":[],\"FileName\":\"TestProject>Program.cs\",\"TypeNames\":[\"Program\"],\"Usings\":[]}]";

        var test = new Test
        {
            TestState =
            {
                ExpectedDiagnostics =
                {
                    DiagnosticResult.CompilerWarning("EMBED0012").WithSpan("/home/source/Program.cs", 3, 30, 3, 63),
                },
                AdditionalFiles =
                {
                    enableMinifyJson,
                },
                Sources = {
                    (
                        "/home/source/Program.cs",
                   // lang=C#
"""
class Program
{
    public int Value { get; [SourceExpander.NotEmbeddingSource] set; }
}
"""
                    ),
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
        await test.RunAsync(cancellationToken);
        await Newtonsoft.Json.JsonConvert.DeserializeObject<SourceFileInfo[]>(embeddedSourceCode)
            .Should().BeEquivalentTo(embeddedFiles);
        await System.Text.Json.JsonSerializer.Deserialize<SourceFileInfo[]>(embeddedSourceCode)
            .Should().BeEquivalentTo(embeddedFiles);
    }

    [Test]
    public async Task PartialClass(CancellationToken cancellationToken)
    {
        var embeddedNamespaces = ImmutableArray<string>.Empty;
        var embeddedFiles = ImmutableArray.Create(
             new SourceFileInfo
             (
                 "TestProject>Program.cs",
                 ["Program"],
                 // lang=C#
                 ImmutableArray.Create("using System;"),
                 ImmutableArray<string>.Empty,
                 // lang=C#
                 "partial class Program{static void Main()=>Console.WriteLine(1);}"
             ));
        const string embeddedSourceCode = "[{\"CodeBody\":\"partial class Program{static void Main()=>Console.WriteLine(1);}\",\"Dependencies\":[],\"FileName\":\"TestProject>Program.cs\",\"TypeNames\":[\"Program\"],\"Usings\":[\"using System;\"]}]";

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
                        "/home/source/Program.cs",
                   // lang=C#
                        """
using System;
partial class Program
{
    static void Main() => Console.WriteLine(1);
}
[SourceExpander.NotEmbeddingSource]
partial class Program
{
    static void M() => Console.WriteLine(2);
}
"""
                    ),
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
        await test.RunAsync(cancellationToken);
        await Newtonsoft.Json.JsonConvert.DeserializeObject<SourceFileInfo[]>(embeddedSourceCode)
            .Should().BeEquivalentTo(embeddedFiles);
        await System.Text.Json.JsonSerializer.Deserialize<SourceFileInfo[]>(embeddedSourceCode)
            .Should().BeEquivalentTo(embeddedFiles);
    }

    [Test]
    public async Task PartialMethod(CancellationToken cancellationToken)
    {
        var embeddedNamespaces = ImmutableArray<string>.Empty;
        var embeddedFiles = ImmutableArray.Create(
             new SourceFileInfo
             (
                 "TestProject>Program.cs",
                 ["Program"],
                 // lang=C#
                 ImmutableArray.Create("using System;"),
                 ImmutableArray<string>.Empty,
                 // lang=C#
                 "partial class Program{static void Main()=>Console.WriteLine(1);}partial class Program{public partial void PartialMethod(){}}"
             ));
        const string embeddedSourceCode = "[{\"CodeBody\":\"partial class Program{static void Main()=>Console.WriteLine(1);}partial class Program{public partial void PartialMethod(){}}\",\"Dependencies\":[],\"FileName\":\"TestProject>Program.cs\",\"TypeNames\":[\"Program\"],\"Usings\":[\"using System;\"]}]";

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
                        "/home/source/Program.cs",
                   // lang=C#
                        """
using System;
partial class Program
{
    static void Main() => Console.WriteLine(1);
    [SourceExpander.NotEmbeddingSource]
    public partial void PartialMethod();
}
partial class Program
{
    public partial void PartialMethod() { }
}
"""
                    ),
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
        await test.RunAsync(cancellationToken);
        await Newtonsoft.Json.JsonConvert.DeserializeObject<SourceFileInfo[]>(embeddedSourceCode)
            .Should().BeEquivalentTo(embeddedFiles);
        await System.Text.Json.JsonSerializer.Deserialize<SourceFileInfo[]>(embeddedSourceCode)
            .Should().BeEquivalentTo(embeddedFiles);
    }

    [Test]
    [Arguments("public Program(Program p){}", DisplayName = "Constructor")]
    [Arguments("static int num = 2;", DisplayName = "Field")]
    [Arguments("int num1=1,num2 = 2;", DisplayName = "Multiple Field")]
    [Arguments("static void M() => Console.WriteLine(2);", DisplayName = "Method")]
    [Arguments("static string text { get; set; }", DisplayName = "Property:Block")]
    [Arguments("int num => 2;", DisplayName = "Property:Expression")]
    [Arguments("class A{}", DisplayName = "class")]
    [Arguments("struct A{}", DisplayName = "struct")]
    [Arguments("struct A{}", DisplayName = "record")]
    [Arguments("record struct A{}", DisplayName = "record struct")]
    [Arguments("enum A{B,C}", DisplayName = "enum")]
    [Arguments("interface A{}", DisplayName = "interface")]
    [Arguments("delegate void A();", DisplayName = "delegate")]
    public async Task Generic(string impl, CancellationToken cancellationToken)
    {
        var embeddedNamespaces = ImmutableArray<string>.Empty;
        var embeddedFiles = ImmutableArray.Create(
             new SourceFileInfo
             (
                 "TestProject>Program.cs",
                 ["Program"],
                 // lang=C#
                 ImmutableArray.Create("using System;"),
                 ImmutableArray<string>.Empty,
                 // lang=C#
                 "class Program{static void Main()=>Console.WriteLine(1);}"
             ));
        const string embeddedSourceCode = "[{\"CodeBody\":\"class Program{static void Main()=>Console.WriteLine(1);}\",\"Dependencies\":[],\"FileName\":\"TestProject>Program.cs\",\"TypeNames\":[\"Program\"],\"Usings\":[\"using System;\"]}]";

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
                        "/home/source/Program.cs",
                        // lang=C#
                        """
using System;
class Program
{
    static void Main() => Console.WriteLine(1);
    [SourceExpander.NotEmbeddingSource]
    /* impl */
}
""".Replace("/* impl */", impl)
                    ),
                },
                GeneratedSources =
                {
                    (typeof(EmbedderGenerator), "EmbeddedSourceCode.Metadata.cs",
                    EnvironmentUtil.JoinByStringBuilder(
                    "// <auto-generated/>",
                    "#pragma warning disable",
                    $"""[assembly: global::System.Reflection.AssemblyMetadataAttribute("SourceExpander.EmbedderVersion","{EmbedderVersion}")]""",
                    $"""[assembly: global::System.Reflection.AssemblyMetadataAttribute("SourceExpander.EmbeddedLanguageVersion","{EmbeddedLanguageVersion}")]""",
                    $"""[assembly: global::System.Reflection.AssemblyMetadataAttribute("SourceExpander.EmbeddedNamespaces","{string.Join(",", embeddedNamespaces)}")]""",
                    $"""[assembly: global::System.Reflection.AssemblyMetadataAttribute("SourceExpander.EmbeddedSourceCode",{embeddedSourceCode.ToLiteral()})]""")
                    ),
                }
            }
        };
        await test.RunAsync(cancellationToken);
        await Newtonsoft.Json.JsonConvert.DeserializeObject<SourceFileInfo[]>(embeddedSourceCode)
            .Should().BeEquivalentTo(embeddedFiles);
        await System.Text.Json.JsonSerializer.Deserialize<SourceFileInfo[]>(embeddedSourceCode)
            .Should().BeEquivalentTo(embeddedFiles);
    }
}
