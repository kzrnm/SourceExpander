using System.Collections.Immutable;
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace SourceExpander.Embedder.Generate.Test
{
    public class AllowUnsafeTest : EmbeddingGeneratorTestBase
    {
        public AllowUnsafeTest()
        {
            compilation = CreateCompilation(TestSyntaxes,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                       .WithAllowUnsafe(true),
                new[] { expanderCoreReference });
            compilation.SyntaxTrees.Should().HaveCount(TestSyntaxes.Length);
            compilation.GetDiagnostics().Should().OnlyContain(d => d.Id == "CS8019");
        }
        private readonly CSharpCompilation compilation;
        private static SyntaxTree[] TestSyntaxes => new[]
        {
                 CSharpSyntaxTree.ParseText(
                    @"using System.Diagnostics;namespace Test{static class Put{public class Nested{ public static void Write(string v){Debug.WriteLine(v);}}}}",
                    path: "/home/source/Put.cs"),
                 CSharpSyntaxTree.ParseText(
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
    }",
                    path: "/home/source/I/D.cs"),
                 CSharpSyntaxTree.ParseText(
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
    }",
                    path: "/home/source/F/N.cs"),
                 CSharpSyntaxTree.ParseText(
       @"
    namespace Test.F
    {
        public enum NumType
        {
            Zero,
            Pos,
            Neg,
        }
    }", path: "/home/source/F/NumType.cs"),
        };
        static readonly CSharpParseOptions parseOptions = new(kind: SourceCodeKind.Regular, documentationMode: DocumentationMode.Parse);
        static readonly ImmutableArray<SourceFileInfo> embeddedFiles
            = ImmutableArray.Create(
                new SourceFileInfo
                (
                    "TestAssembly>F/N.cs",
                    new string[] { "Test.F.N" },
                    new string[] { "using System;", "using System.Diagnostics;", "using static System.Console;" },
                    new string[] { "TestAssembly>F/NumType.cs", "TestAssembly>Put.cs" },
                    "namespace Test.F{class N{public static void WriteN(){Console.Write(NumType.Zero);Write(\"N\");Trace.Write(\"N\");Put.Nested.Write(\"N\");}}}"
                ), new SourceFileInfo
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

        [Fact]
        public void GenerateTest()
        {
            var generator = new EmbedderGenerator();
            var gen = RunGenerator(compilation, generator, additionalTexts: new[] { enableMinifyJson }, parseOptions: parseOptions);
            gen.Diagnostics.Should().BeEmpty();
            gen.OutputCompilation.GetDiagnostics().Should().OnlyContain(d => d.Id == "CS8019");
            gen.OutputCompilation.SyntaxTrees.Should().HaveCount(TestSyntaxes.Length + 2);

            var metadata = gen.OutputCompilation.Assembly.GetAttributes()
                .Where(x => x.AttributeClass?.Name == nameof(System.Reflection.AssemblyMetadataAttribute))
                .ToDictionary(x => (string)x.ConstructorArguments[0].Value, x => (string)x.ConstructorArguments[1].Value);
            metadata.Should().NotContainKey("SourceExpander.EmbeddedSourceCode");
            metadata.Should().ContainKey("SourceExpander.EmbeddedSourceCode.GZipBase32768");

            var embedded = metadata["SourceExpander.EmbeddedSourceCode.GZipBase32768"];
            Newtonsoft.Json.JsonConvert.DeserializeObject<SourceFileInfo[]>(
                SourceFileInfoUtil.FromGZipBase32768(embedded))
                .Should()
                .BeEquivalentTo(embeddedFiles);
            System.Text.Json.JsonSerializer.Deserialize<SourceFileInfo[]>(
                SourceFileInfoUtil.FromGZipBase32768(embedded))
                .Should()
                .BeEquivalentTo(embeddedFiles);

            gen.Diagnostics.Should().BeEmpty();
            gen.AddedSyntaxTrees
                .Should()
                .ContainSingle(t => t.FilePath.EndsWith("EmbeddedSourceCode.Metadata.cs"))
                .Which
                .ToString()
                .Should()
                .ContainAll(
                "[assembly: AssemblyMetadataAttribute(\"SourceExpander.EmbeddedSourceCode.GZipBase32768\",",
                "[assembly: AssemblyMetadataAttribute(\"SourceExpander.EmbedderVersion\",",
                "[assembly: AssemblyMetadataAttribute(\"SourceExpander.EmbeddedAllowUnsafe\",\"true\")");
        }
    }
}
