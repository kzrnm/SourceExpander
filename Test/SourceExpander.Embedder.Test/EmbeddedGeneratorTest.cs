extern alias Core;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace SourceExpander.Embedder.Test
{
    public class EmbeddedGeneratorTest
    {
        [Fact]
        public void GenerateTest()
        {
            var compilation = CSharpCompilation.Create(
                assemblyName: "TestAssembly",
                syntaxTrees: GetSyntaxes(),
                references: defaultMetadatas.Append(expanderCoreReference),
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            compilation.SyntaxTrees.Should().HaveCount(TestSyntaxes.Length);
            compilation.GetDiagnostics().Should().BeEmpty();

            var generator = new EmbeddedGenerator();
            var driver = CSharpGeneratorDriver.Create(new[] { generator }, parseOptions: new CSharpParseOptions(kind: SourceCodeKind.Regular, documentationMode: DocumentationMode.Parse));
            driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);
            diagnostics.Should().BeEmpty();
            outputCompilation.SyntaxTrees.Should().HaveCount(TestSyntaxes.Length + 1);

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

            outputCompilation
                .GetTypeByMetadataName("System.Runtime.CompilerServices.ModuleInitializerAttribute")
                .Should()
                .NotBeNull();

            outputCompilation
                .GetTypeByMetadataName("ModuleInitializer")
                .Should()
                .NotBeNull();

            var newTree = outputCompilation.SyntaxTrees
                .Should()
                .ContainSingle(tree => tree.GetRoot(default).ToString().Contains("internal static class ModuleInitializer"))
                .Which;
            newTree.GetDiagnostics().Should().BeEmpty();
        }

        [Fact]
        public void GenerateErrorTest()
        {
            var compilation = CSharpCompilation.Create(
                assemblyName: "TestAssembly",
                syntaxTrees: GetSyntaxes(),
                references: defaultMetadatas,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            compilation.SyntaxTrees.Should().HaveCount(TestSyntaxes.Length);
            compilation.GetDiagnostics()
                .Should().BeEmpty();

            var generator = new EmbeddedGenerator();
            var driver = CSharpGeneratorDriver.Create(new[] { generator }, parseOptions: new CSharpParseOptions(kind: SourceCodeKind.Regular, documentationMode: DocumentationMode.Parse));
            driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);
            outputCompilation.SyntaxTrees
                .Should().HaveCount(TestSyntaxes.Length);

            var diagnostic = diagnostics.Should().ContainSingle().Which;
            diagnostic.Id.Should().Be("EMBED0001");
            diagnostic.DefaultSeverity.Should().Be(DiagnosticSeverity.Error);
            diagnostic.Descriptor.Title.ToString()
                .Should()
                .Contain("need class SourceExpander.SourceFileInfo");
        }

        static readonly MetadataReference[] defaultMetadatas = GetDefaulMetadatas().ToArray();
        static readonly MetadataReference expanderCoreReference = MetadataReference.CreateFromFile(typeof(Core.SourceExpander.SourceFileInfo).Assembly.Location);

        static IEnumerable<MetadataReference> GetDefaulMetadatas()
        {
            var directory = Path.GetDirectoryName(typeof(object).Assembly.Location);
            foreach (var file in Directory.EnumerateFiles(directory, "System*.dll"))
            {
                yield return MetadataReference.CreateFromFile(file);
            }
        }

        static readonly SyntaxTree[] TestSyntaxes = GetSyntaxes().ToArray();
        static IEnumerable<SyntaxTree> GetSyntaxes()
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
