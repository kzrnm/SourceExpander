using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SourceExpander.Expanded;
using Xunit;

namespace SourceExpander.Generator.Generate.Test
{
    public class AllowUnsafeTest : ExpandGeneratorTestBase
    {
        [Fact]
        public void NotAllowTest()
        {
            var syntaxTrees = new[]
            {
                CSharpSyntaxTree.ParseText(
                    @"using System;
using SampleLibrary;

class Program
{
    static void Main()
    {
        Console.WriteLine(42);
        Put.WriteRandom();
#if !EXPAND_GENERATOR
        Console.WriteLine(24);
#endif
    }
}",
                    options: new CSharpParseOptions(documentationMode:DocumentationMode.None),
                    path: "/home/source/Program.cs"),
            };
            var anotherCompilation = CreateCompilation(
                new[] {
                    CSharpSyntaxTree.ParseText(
                        @"[assembly: System.Reflection.AssemblyMetadata(""SourceExpander.EmbeddedSourceCode"", ""[{\""CodeBody\"":\""namespace Other { public static class C { public static void P() => System.Console.WriteLine(); } } \"",\""Dependencies\"":[],\""FileName\"":\""OtherDependency>C.cs\"",\""TypeNames\"":[\""Other.C\""],\""Usings\"":[]}]"")]"
                        + @"[assembly: System.Reflection.AssemblyMetadata(""SourceExpander.EmbedderVersion"",""1.1.1.1"")]"
                        + @"[assembly: System.Reflection.AssemblyMetadata(""SourceExpander.EmbeddedAllowUnsafe"",""true"")]",
                        path: @"/home/other/AssemblyInfo.cs"),
                },
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
                additionalMetadatas: sampleLibReferences.Append(coreReference),
                assemblyName: "OtherDependency"
                );

            var compilation = CreateCompilation(
                syntaxTrees,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                    .WithSpecificDiagnosticOptions(new Dictionary<string, ReportDiagnostic> {
                        { "CS8019", ReportDiagnostic.Suppress },
                    }),
                additionalMetadatas: sampleLibReferences.Append(coreReference).Append(anotherCompilation.ToMetadataReference())
                );
            compilation.SyntaxTrees.Should().HaveCount(syntaxTrees.Length);

            var generator = new ExpandGenerator();
            var gen = RunGenerator(compilation, generator,
                parseOptions: new CSharpParseOptions(kind: SourceCodeKind.Regular, documentationMode: DocumentationMode.Parse));
            gen.OutputCompilation.SyntaxTrees.Should().HaveCount(syntaxTrees.Length + 1);

            gen.OutputCompilation.SyntaxTrees
                .Should()
                .ContainSingle(tree => tree.FilePath.EndsWith("SourceExpander.Expanded.cs"));
            var files = GetExpandedFilesWithCore(gen.OutputCompilation);
            files.Should().HaveCount(1);
            files["/home/source/Program.cs"].Should()
                .BeEquivalentTo(
                new SourceCode(
                    path: "/home/source/Program.cs",
                    code: @"using SampleLibrary;
using System;
using System.Diagnostics;
class Program
{
    static void Main()
    {
        Console.WriteLine(42);
        Put.WriteRandom();
#if !EXPAND_GENERATOR
        Console.WriteLine(24);
#endif
    }
}
#region Expanded by https://github.com/naminodarie/SourceExpander
namespace SampleLibrary { public static class Put { private static readonly Xorshift rnd = new Xorshift(); public static void WriteRandom() => Trace.WriteLine(rnd.Next()); } } 
namespace SampleLibrary { public class Xorshift : Random { private uint x = 123456789; private uint y = 362436069; private uint z = 521288629; private uint w; private static readonly Random rnd = new Random(); public Xorshift() : this(rnd.Next()) { } public Xorshift(int seed) { w = (uint)seed; } protected override double Sample() => InternalSample() * (1.0 / uint.MaxValue); private uint InternalSample() { uint t = x ^ (x << 11); x = y; y = z; z = w; return w = (w ^ (w >> 19)) ^ (t ^ (t >> 8)); } } } 
#endregion Expanded by https://github.com/naminodarie/SourceExpander
")
                );

            gen.OutputCompilation.GetDiagnostics().Should().BeEmpty();
            var diagnostic = gen.Diagnostics
                .Should()
                .ContainSingle()
                .Which;
            diagnostic.Id.Should().Be("EXPAND0006");
            diagnostic.DefaultSeverity.Should().Be(DiagnosticSeverity.Warning);
            diagnostic.GetMessage()
                .Should()
                .Be("maybe needs AllowUnsafeBlocks because OtherDependency has AllowUnsafeBlocks");
        }

        [Fact]
        public void AllowTest()
        {
            var syntaxTrees = new[]
            {
                CSharpSyntaxTree.ParseText(
                    @"using System;
using SampleLibrary;

class Program
{
    static void Main()
    {
        Console.WriteLine(42);
        Put.WriteRandom();
#if !EXPAND_GENERATOR
        Console.WriteLine(24);
#endif
    }
}",
                    options: new CSharpParseOptions(documentationMode:DocumentationMode.None),
                    path: "/home/source/Program.cs"),
            };
            var anotherCompilation = CreateCompilation(
                new[] {
                    CSharpSyntaxTree.ParseText(
                        @"[assembly: System.Reflection.AssemblyMetadata(""SourceExpander.EmbeddedSourceCode"", ""[{\""CodeBody\"":\""namespace Other { public static class C { public static void P() => System.Console.WriteLine(); } } \"",\""Dependencies\"":[],\""FileName\"":\""OtherDependency>C.cs\"",\""TypeNames\"":[\""Other.C\""],\""Usings\"":[]}]"")]"
                        + @"[assembly: System.Reflection.AssemblyMetadata(""SourceExpander.EmbedderVersion"",""1.1.1.1"")]"
                        + @"[assembly: System.Reflection.AssemblyMetadata(""SourceExpander.EmbeddedAllowUnsafe"",""true"")]",
                        path: @"/home/other/AssemblyInfo.cs"),
                },
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
                additionalMetadatas: sampleLibReferences.Append(coreReference),
                assemblyName: "OtherDependency"
                );

            var compilation = CreateCompilation(
                syntaxTrees,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                    .WithAllowUnsafe(true)
                    .WithSpecificDiagnosticOptions(new Dictionary<string, ReportDiagnostic> {
                        { "CS8019", ReportDiagnostic.Suppress },
                    }),
                additionalMetadatas: sampleLibReferences.Append(coreReference).Append(anotherCompilation.ToMetadataReference())
                );
            compilation.SyntaxTrees.Should().HaveCount(syntaxTrees.Length);

            var generator = new ExpandGenerator();
            var gen = RunGenerator(compilation, generator
                , parseOptions: new CSharpParseOptions(kind: SourceCodeKind.Regular, documentationMode: DocumentationMode.Parse));
            gen.OutputCompilation.SyntaxTrees.Should().HaveCount(syntaxTrees.Length + 1);

            gen.OutputCompilation.SyntaxTrees
                .Should()
                .ContainSingle(tree => tree.FilePath.EndsWith("SourceExpander.Expanded.cs"));
            var files = GetExpandedFilesWithCore(gen.OutputCompilation);
            files.Should().HaveCount(1);
            files["/home/source/Program.cs"].Should()
                .BeEquivalentTo(
                new SourceCode(
                    path: "/home/source/Program.cs",
                    code: @"using SampleLibrary;
using System;
using System.Diagnostics;
class Program
{
    static void Main()
    {
        Console.WriteLine(42);
        Put.WriteRandom();
#if !EXPAND_GENERATOR
        Console.WriteLine(24);
#endif
    }
}
#region Expanded by https://github.com/naminodarie/SourceExpander
namespace SampleLibrary { public static class Put { private static readonly Xorshift rnd = new Xorshift(); public static void WriteRandom() => Trace.WriteLine(rnd.Next()); } } 
namespace SampleLibrary { public class Xorshift : Random { private uint x = 123456789; private uint y = 362436069; private uint z = 521288629; private uint w; private static readonly Random rnd = new Random(); public Xorshift() : this(rnd.Next()) { } public Xorshift(int seed) { w = (uint)seed; } protected override double Sample() => InternalSample() * (1.0 / uint.MaxValue); private uint InternalSample() { uint t = x ^ (x << 11); x = y; y = z; z = w; return w = (w ^ (w >> 19)) ^ (t ^ (t >> 8)); } } } 
#endregion Expanded by https://github.com/naminodarie/SourceExpander
")
                );

            gen.OutputCompilation.GetDiagnostics().Should().BeEmpty();
            gen.Diagnostics.Should().BeEmpty();
        }
    }
}
