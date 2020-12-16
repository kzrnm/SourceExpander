using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SourceExpander.Expanded;
using Xunit;

namespace SourceExpander.Generator.Generate.Test
{
    public class DefaultWithCoreReferenceTest : ExpandGeneratorTestBase
    {
        private static SyntaxTree[] CreateTrees(LanguageVersion languageVersion)
        {
            var opts = new CSharpParseOptions(documentationMode: DocumentationMode.None)
                .WithLanguageVersion(languageVersion);
            return new[]
            {
                CSharpSyntaxTree.ParseText(
                    @"using System;
using SampleLibrary;

class Program
{
    static void P()
    {
        Console.WriteLine(42);
        Put.WriteRandom();
#if !EXPAND_GENERATOR
        Console.WriteLine(24);
#endif
    }
}",
                    options: opts,
                    path: "/home/source/Program.cs"),
                CSharpSyntaxTree.ParseText(
                    @"using System;
using System.Reflection;
using SampleLibrary;
using static System.MathF;
using M = System.Math;


Console.WriteLine(42);
Put2.Write();",
                    options: opts,
                    path: "/home/source/Program2.cs"),
            };
        }

        [Fact]
        public void SuccessConsoleApp()
        {
            var version = LanguageVersion.Latest;
            var syntaxTrees = CreateTrees(version);
            var compilation = CreateCompilation(
                syntaxTrees,
                new CSharpCompilationOptions(OutputKind.ConsoleApplication)
                    .WithSpecificDiagnosticOptions(new Dictionary<string, ReportDiagnostic> {
                        { "CS8019", ReportDiagnostic.Suppress },
                    }),
                additionalMetadatas: sampleLibReferences.Append(coreReference)
                );
            compilation.SyntaxTrees.Should().HaveCount(syntaxTrees.Length);

            var generator = new ExpandGenerator();
            var gen = RunGenerator(compilation, generator,
                parseOptions: new CSharpParseOptions(kind: SourceCodeKind.Regular, documentationMode: DocumentationMode.Parse, languageVersion: version));
            gen.Diagnostics.Should().BeEmpty();
            gen.OutputCompilation.GetDiagnostics().Should().BeEmpty();
            gen.OutputCompilation.SyntaxTrees.Should().HaveCount(syntaxTrees.Length + 1);
            gen.OutputCompilation.SyntaxTrees
                .Should()
                .ContainSingle(tree => tree.FilePath.EndsWith("SourceExpander.Expanded.cs"));
            var files = GetExpandedFilesWithCore(gen.OutputCompilation);
            files.Should().HaveCount(2);
            files["/home/source/Program.cs"].Should()
                .BeEquivalentTo(
                new SourceCode(
                    path: "/home/source/Program.cs",
                    code: @"using SampleLibrary;
using System;
using System.Diagnostics;
class Program
{
    static void P()
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
            files["/home/source/Program2.cs"].Should()
                .BeEquivalentTo(
                new SourceCode(
                    path: "/home/source/Program2.cs",
                    code: @"using SampleLibrary;
using System;
using System.Diagnostics;
Console.WriteLine(42);
Put2.Write();
#region Expanded by https://github.com/naminodarie/SourceExpander
namespace SampleLibrary { public static class Put2 { public static void Write() => Put.WriteRandom(); } } 
namespace SampleLibrary { public static class Put { private static readonly Xorshift rnd = new Xorshift(); public static void WriteRandom() => Trace.WriteLine(rnd.Next()); } } 
namespace SampleLibrary { public class Xorshift : Random { private uint x = 123456789; private uint y = 362436069; private uint z = 521288629; private uint w; private static readonly Random rnd = new Random(); public Xorshift() : this(rnd.Next()) { } public Xorshift(int seed) { w = (uint)seed; } protected override double Sample() => InternalSample() * (1.0 / uint.MaxValue); private uint InternalSample() { uint t = x ^ (x << 11); x = y; y = z; z = w; return w = (w ^ (w >> 19)) ^ (t ^ (t >> 8)); } } } 
#endregion Expanded by https://github.com/naminodarie/SourceExpander
")
                );
        }


        [Theory]
        [InlineData(LanguageVersion.CSharp4)]
        [InlineData(LanguageVersion.Latest)]
        public void SuccessVersion(LanguageVersion version)
        {
            var syntaxTrees = CreateTrees(version).Take(1).ToArray();
            var compilation = CreateCompilation(
                syntaxTrees,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                    .WithSpecificDiagnosticOptions(new Dictionary<string, ReportDiagnostic> {
                        { "CS8019", ReportDiagnostic.Suppress },
                    }),
                additionalMetadatas: sampleLibReferences.Append(coreReference)
                );
            compilation.SyntaxTrees.Should().HaveCount(syntaxTrees.Length);

            var generator = new ExpandGenerator();
            var gen = RunGenerator(compilation, generator,
                parseOptions: new CSharpParseOptions(kind: SourceCodeKind.Regular, documentationMode: DocumentationMode.Parse, languageVersion: version));
            gen.Diagnostics.Should().BeEmpty();
            gen.OutputCompilation.GetDiagnostics().Should().BeEmpty();
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
    static void P()
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
        }

        [Theory]
        [InlineData(LanguageVersion.CSharp1)]
        [InlineData(LanguageVersion.CSharp2)]
        [InlineData(LanguageVersion.CSharp3)]
        public void FailureVersion(LanguageVersion version)
        {

            var syntaxTrees = CreateTrees(version).Take(1).ToArray();
            var compilation = CreateCompilation(
                syntaxTrees,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                    .WithSpecificDiagnosticOptions(new Dictionary<string, ReportDiagnostic> {
                        { "CS8019", ReportDiagnostic.Suppress },
                    }),
                additionalMetadatas: sampleLibReferences.Append(coreReference)
                );
            compilation.SyntaxTrees.Should().HaveCount(syntaxTrees.Length);

            var generator = new ExpandGenerator();
            var gen = RunGenerator(compilation, generator,
                parseOptions: new CSharpParseOptions(kind: SourceCodeKind.Regular, documentationMode: DocumentationMode.Parse, languageVersion: version));
            gen.Diagnostics.Should()
                .ContainSingle()
                .Which
                .Id
                .Should()
                .Be("EXPAND0004");
        }
    }
}
