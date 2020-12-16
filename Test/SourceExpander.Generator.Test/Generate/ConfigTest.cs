using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SourceExpander.Expanded;
using Xunit;

namespace SourceExpander.Generator.Generate.Test
{
    public class ConfigTest : ExpandGeneratorTestBase
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

        public static readonly TheoryData ParseErrorJsons = new TheoryData<InMemoryAdditionalText>
        {
            {
                new InMemoryAdditionalText("/foo/bar/SourceExpander.Generator.Config.json", @"
{
    ""$schema"": ""https://raw.githubusercontent.com/naminodarie/SourceExpander/master/schema/expander.schema.json"",
    ""ignore-file-pattern-regex"": 1
}
")
            },
            {
                new InMemoryAdditionalText("/foo/bar/sourceExpander.generator.config.json", @"
{
    ""$schema"": ""https://raw.githubusercontent.com/naminodarie/SourceExpander/master/schema/expander.schema.json"",
    ""ignore-file-pattern-regex"": 1
}
")
            },
            {
                new InMemoryAdditionalText("/regexerror/SourceExpander.Generator.Config.json", @"
{
    ""$schema"": ""https://raw.githubusercontent.com/naminodarie/SourceExpander/master/schema/expander.schema.json"",
    ""ignore-file-pattern-regex"": [
        ""(""
    ]
}
")
            },
        };

        [Theory]
        [MemberData(nameof(ParseErrorJsons))]
        public void ParseErrorTest(InMemoryAdditionalText additionalText)
        {
            var version = LanguageVersion.Latest;
            var syntaxTrees = CreateTrees(version);
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
                additionalTexts: new AdditionalText[] {
                    additionalText,
                    new InMemoryAdditionalText("/foo/bar/SourceExpander.Notmatch.json", "notmatch"),
                },
                parseOptions: new CSharpParseOptions(kind: SourceCodeKind.Regular, documentationMode: DocumentationMode.Parse, languageVersion: version));
            gen.Diagnostics.Should().ContainSingle().Which.Id.Should().Be("EXPAND0007");
        }

        [Fact]
        public void NotEnabled()
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

            var additionalText = new InMemoryAdditionalText(
                "/foo/bar/SourceExpander.Generator.Config.json", @"
{
    ""$schema"": ""https://raw.githubusercontent.com/naminodarie/SourceExpander/master/schema/expander.schema.json"",
    ""enabled"": false
}
");

            var generator = new ExpandGenerator();
            var gen = RunGenerator(compilation, generator,
                additionalTexts: new AdditionalText[] { additionalText },
                parseOptions: new CSharpParseOptions(kind: SourceCodeKind.Regular, documentationMode: DocumentationMode.Parse, languageVersion: version));
            gen.Diagnostics.Should().BeEmpty();
            gen.OutputCompilation.GetDiagnostics().Should().BeEmpty();
            gen.OutputCompilation.SyntaxTrees.Should().HaveCount(syntaxTrees.Length);
            gen.OutputCompilation.SyntaxTrees
                .Should()
                .NotContain(tree => tree.FilePath.EndsWith("SourceExpander.Expanded.cs"));
            gen.OutputCompilation.GetTypeByMetadataName("SourceExpander.Expanded.ExpandedContainer").Should().BeNull();
        }

        [Fact]
        public void Whitelist()
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

            var pattern = @"source/Program.cs";
            var additionalText = new InMemoryAdditionalText(
                "/foo/bar/SourceExpander.Generator.Config.json", @"
{
    ""$schema"": ""https://raw.githubusercontent.com/naminodarie/SourceExpander/master/schema/expander.schema.json"",
    ""match-file-pattern"": [
" + pattern.ToLiteral() + @"
    ]
}
");

            var generator = new ExpandGenerator();
            var gen = RunGenerator(compilation, generator,
                additionalTexts: new AdditionalText[] { additionalText },
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


        [Fact]
        public void IgnoreFile()
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

            // language=regex
            var pattern = @"source/Program\d+\.cs$";
            var additionalText = new InMemoryAdditionalText(
                "/foo/bar/SourceExpander.Generator.Config.json", @"
{
    ""$schema"": ""https://raw.githubusercontent.com/naminodarie/SourceExpander/master/schema/expander.schema.json"",
    ""ignore-file-pattern-regex"": [
" + pattern.ToLiteral() + @"
    ]
}
");

            var generator = new ExpandGenerator();
            var gen = RunGenerator(compilation, generator,
                additionalTexts: new AdditionalText[] { additionalText },
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
    }
}
