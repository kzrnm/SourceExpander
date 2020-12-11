using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SourceExpander.Expanded;
using Xunit;

namespace SourceExpander.Generator.Generate.Test
{
    public class OlderVersionTest : ExpandGeneratorTestBase
    {
        [Fact]
        public void GenerateTest()
        {
            var newerEmbedderCompilation = CreateCompilation(
                new[] {
                    CSharpSyntaxTree.ParseText(
                        @"[assembly: System.Reflection.AssemblyMetadata(""SourceExpander.EmbeddedSourceCode"", ""[{\""CodeBody\"":\""namespace Other { public static class C { public static void P() => System.Console.WriteLine(); } } \"",\""Dependencies\"":[],\""FileName\"":\""OtherDependency>C.cs\"",\""TypeNames\"":[\""Other.C\""],\""Usings\"":[]}]"")]"
                        + @"[assembly: System.Reflection.AssemblyMetadata(""SourceExpander.EmbedderVersion"",""2147483647.2147483647.2147483647.2147483647"")]",
                        path: @"/home/other/AssemblyInfo.cs"),
                },
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
                additionalMetadatas: new[] { coreReference },
                assemblyName: "OtherDependency");
            var compilation = CreateCompilation(
                new[]
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
                },
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                    .WithSpecificDiagnosticOptions(new Dictionary<string, ReportDiagnostic> {
                        { "CS8019", ReportDiagnostic.Suppress },
                    }),
                additionalMetadatas: sampleLibReferences.Append(coreReference).Append(newerEmbedderCompilation.ToMetadataReference()));
            compilation.SyntaxTrees.Should().HaveCount(1);

            var generator = new ExpandGenerator();
            var driver = CSharpGeneratorDriver.Create(new[] { generator }, parseOptions: new CSharpParseOptions(kind: SourceCodeKind.Regular, documentationMode: DocumentationMode.Parse));
            driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);
            outputCompilation.SyntaxTrees.Should().HaveCount(2);

            outputCompilation.SyntaxTrees
                .Should()
                .ContainSingle(tree => tree.FilePath.EndsWith("SourceExpander.Expanded.cs"));
            var files = GetExpandedFilesWithCore(outputCompilation);
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
#region Expanded
namespace SampleLibrary { public static class Put { private static readonly Xorshift rnd = new Xorshift(); public static void WriteRandom() => Trace.WriteLine(rnd.Next()); } } 
namespace SampleLibrary { public class Xorshift : Random { private uint x = 123456789; private uint y = 362436069; private uint z = 521288629; private uint w; private static readonly Random rnd = new Random(); public Xorshift() : this(rnd.Next()) { } public Xorshift(int seed) { w = (uint)seed; } protected override double Sample() => InternalSample() * (1.0 / uint.MaxValue); private uint InternalSample() { uint t = x ^ (x << 11); x = y; y = z; z = w; return w = (w ^ (w >> 19)) ^ (t ^ (t >> 8)); } } } 
#endregion Expanded
")
                );

            outputCompilation.GetDiagnostics().Should().BeEmpty();
            var diagnostic = diagnostics
                .Should()
                .ContainSingle()
                .Which;
            diagnostic.Id.Should().Be("EXPAND0002");
            diagnostic.DefaultSeverity.Should().Be(DiagnosticSeverity.Warning);
            diagnostic.GetMessage()
                .Should()
                .MatchRegex(
                    // language=regex
                    @"Expander version\(\d+\.\d+\.\d+\.\d+\) is older than embedder of OtherDependency\(2147483647\.2147483647\.2147483647\.2147483647\)");
        }
    }
}
