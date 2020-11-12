extern alias Core;

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Newtonsoft.Json;
using Xunit;
using static SourceExpander.Embedder.Test.Util;

namespace SourceExpander.Embedder.Test
{
    public class EmbeddedGeneratorTest
    {
        [Fact]
        public void GenerateTest()
        {
            var compilation = CSharpCompilation.Create(
                assemblyName: "TestAssembly",
                syntaxTrees: GetTestSyntaxes(),
                references: defaultMetadatas.Append(expanderCoreReference),
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            compilation.SyntaxTrees.Should().HaveCount(TestSyntaxesCount);
            compilation.GetDiagnostics().Should().BeEmpty();

            var generator = new EmbeddedGenerator();
            var driver = CSharpGeneratorDriver.Create(new[] { generator }, parseOptions: new CSharpParseOptions(kind: SourceCodeKind.Regular, documentationMode: DocumentationMode.Parse));
            driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);
            diagnostics.Should().BeEmpty();
            outputCompilation.SyntaxTrees.Should().HaveCount(TestSyntaxesCount + 1);

            generator.ResolveFiles(compilation)
                .Should()
                .BeEquivalentTo(
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
                });

            var metadata = outputCompilation.Assembly.GetAttributes()
                .Where(x => x.AttributeClass?.Name == nameof(System.Reflection.AssemblyMetadataAttribute))
                .ToDictionary(x => (string)x.ConstructorArguments[0].Value, x => (string)x.ConstructorArguments[1].Value);
            metadata.Should().ContainKey("SourceExpander.EmbeddedSourceCode");
            JsonConvert.DeserializeObject<SourceFileInfo[]>(metadata["SourceExpander.EmbeddedSourceCode"])
                .Should()
                .BeEquivalentTo(
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
                });

            var newTree = outputCompilation.SyntaxTrees
                .Should()
                .ContainSingle(tree => tree.GetRoot(default).ToString().Contains("[assembly: System.Reflection.AssemblyMetadataAttribute"))
                .Which;

            newTree.GetDiagnostics().Should().BeEmpty();
        }

        [Fact]
        public void GenerateNoSourceInfoClassTest()
        {
            var compilation = CSharpCompilation.Create(
                assemblyName: "TestAssembly",
                syntaxTrees: GetTestSyntaxes(),
                references: defaultMetadatas,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            compilation.SyntaxTrees.Should().HaveCount(TestSyntaxesCount);
            compilation.GetDiagnostics()
                .Should().BeEmpty();

            var generator = new EmbeddedGenerator();
            var driver = CSharpGeneratorDriver.Create(new[] { generator }, parseOptions: new CSharpParseOptions(kind: SourceCodeKind.Regular, documentationMode: DocumentationMode.Parse));
            driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);
            outputCompilation.SyntaxTrees
                .Should().HaveCount(TestSyntaxesCount + 1);

            diagnostics.Should().BeEmpty();
        }

        static readonly int TestSyntaxesCount = GetTestSyntaxes().Count();
        static IEnumerable<SyntaxTree> GetTestSyntaxes()
        {
            yield return CSharpSyntaxTree.ParseText(
                @"using System.Diagnostics;namespace Test{static class Put{public class Nested{ public static void Write(string v){Debug.WriteLine(v);}}}}",
                path: "/home/source/Put.cs");
            yield return CSharpSyntaxTree.ParseText(
                @"using System.Diagnostics;
using System;
using System.Collections.Generic;
namespace Test.I
{
    class D<T> : IComparer<T>
    {
        public int Compare(T x, T y) => throw new NotImplementedException();
        public static void WriteType()
        {
            Console.Write(typeof(T).FullName);
            Trace.Write(typeof(T).FullName);
            Put.Nested.Write(typeof(T).FullName);
        }
    }
}",
                path: "/home/source/I/D.cs");
            yield return CSharpSyntaxTree.ParseText(
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
                path: "/home/source/F/N.cs");
            yield return CSharpSyntaxTree.ParseText(
   @"
namespace Test.F
{
    public enum NumType
    {
        Zero,
        Pos,
        Neg,
    }
}", path: "/home/source/F/NumType.cs");
        }
    }
}
