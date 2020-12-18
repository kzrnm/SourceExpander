using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SourceExpander.Expanded;
using Xunit;

namespace SourceExpander.Generator.Generate.Test
{
    public class UsingRemoverTest : ExpandGeneratorTestBase
    {
        [Fact]
        public void GenerateTest()
        {
            var compilation = CreateCompilation(
                new[]
                {
                    CSharpSyntaxTree.ParseText(
                    @"using System;
using SampleLibrary;
using System.Collections;
using static System.StringSplitOptions;
using static SampleLibrary.Put;
using static SampleLibrary.Put2;
using Li = System.Collections.Generic.List<int>;

namespace Name
{
    using System.Collections.Generic;
    using static System.Base64FormattingOptions;
    using E = System.Linq.Enumerable;
    class Program
    {
        static void Main()
        {
            Console.WriteLine(new Xorshift().Next());
            WriteRandom();
    #if !EXPAND_GENERATOR
            Console.WriteLine(24);
    #endif
        }
    }
}",
                        options: new CSharpParseOptions(documentationMode:DocumentationMode.None),
                        path: "/home/source/Program.cs"),
                },
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                    .WithSpecificDiagnosticOptions(new Dictionary<string, ReportDiagnostic> {
                        { "CS8019", ReportDiagnostic.Suppress },
                    }),
                additionalMetadatas: sampleLibReferences.Append(coreReference));
            compilation.SyntaxTrees.Should().HaveCount(1);

            var generator = new ExpandGenerator();
            var gen = RunGenerator(compilation, generator, parseOptions: new CSharpParseOptions(kind: SourceCodeKind.Regular, documentationMode: DocumentationMode.Parse));
            gen.OutputCompilation.SyntaxTrees.Should().HaveCount(2);

            gen.AddedSyntaxTrees
                .Should()
                .ContainSingle();
            var files = GetExpandedFilesWithCore(gen.OutputCompilation);
            files.Should().HaveCount(1);
            files["/home/source/Program.cs"].Should()
                .BeEquivalentTo(
                new SourceCode(
                    path: "/home/source/Program.cs",
                    code: @"using SampleLibrary;
using System;
using System.Diagnostics;
using static SampleLibrary.Put;
namespace Name
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine(new Xorshift().Next());
            WriteRandom();
    #if !EXPAND_GENERATOR
            Console.WriteLine(24);
    #endif
        }
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
