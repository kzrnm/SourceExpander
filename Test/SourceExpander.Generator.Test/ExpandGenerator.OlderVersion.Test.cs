using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace SourceExpander.Generator.Test
{
    public class ExpandGeneratorOlderVersionTest
    {
        [Fact]
        public void GenerateTest()
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
                CSharpSyntaxTree.ParseText(
                    @"using System;
using System.Reflection;
using SampleLibrary;
using static System.MathF;
using M = System.Math;

class Program2
{
    static void Main()
    {
        Console.WriteLine(42);
        Put2.Write();
    }
}",
                    options: new CSharpParseOptions(documentationMode:DocumentationMode.None),
                    path: "/home/source/Program2.cs"),
            };
            var newerEmbedderCompilation = CSharpCompilation.Create("OtherDependency",
                syntaxTrees: new[] {
                    CSharpSyntaxTree.ParseText(
                        @"[assembly: System.Reflection.AssemblyMetadata(""SourceExpander.EmbeddedSourceCode"", ""[{\""CodeBody\"":\""namespace Other { public static class C { public static void P() => System.Console.WriteLine(); } } \"",\""Dependencies\"":[],\""FileName\"":\""OtherDependency>C.cs\"",\""TypeNames\"":[\""Other.C\""],\""Usings\"":[]}]"")]"
                        + @"[assembly: System.Reflection.AssemblyMetadata(""SourceExpander.EmbedderVersion"",""2147483647.2147483647.2147483647.2147483647"")]",
                        path: @"/home/other/AssemblyInfo.cs"),
                },
                references: TestUtil.noCoreReferenceMetadatas,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            );

            var sampleReferences = TestUtil.GetSampleDllPaths().Select(path => MetadataReference.CreateFromFile(path));
            var compilation = CSharpCompilation.Create(
                assemblyName: "TestAssembly",
                syntaxTrees: syntaxTrees,
                references: TestUtil.noCoreReferenceMetadatas.Concat(sampleReferences).Append(newerEmbedderCompilation.ToMetadataReference()),
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithSpecificDiagnosticOptions(new Dictionary<string, ReportDiagnostic> {
                    { "CS8019", ReportDiagnostic.Suppress },
                }));
            compilation.SyntaxTrees.Should().HaveCount(syntaxTrees.Length);

            var generator = new ExpandGenerator();
            var driver = CSharpGeneratorDriver.Create(new[] { generator }, parseOptions: new CSharpParseOptions(kind: SourceCodeKind.Regular, documentationMode: DocumentationMode.Parse));
            driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);
            outputCompilation.SyntaxTrees.Should().HaveCount(syntaxTrees.Length + 1);
            var d = outputCompilation.SyntaxTrees
                .Should()
                .ContainSingle(tree => tree.FilePath.EndsWith("SourceExpander.Expanded.cs"))
                .Which
                .ToString()
                .Replace("\r\n", "\n")
                .Replace("\\r\\n", "\\n")
                .Should()
                .Be("using System.Collections.Generic;\n" +
                    "namespace SourceExpander.Expanded{\n" +
                        "public class SourceCode{ public string Path; public string Code; }\n" +
                        "public static class Expanded{\n" +
                        "public static IReadOnlyDictionary<string, SourceCode> Files { get; } = new Dictionary<string, SourceCode>{\n" +
                        "{\"/home/source/Program.cs\", new SourceCode{ Path=\"/home/source/Program.cs\", " +
                            "Code=\"using SampleLibrary;\\nusing System;\\nusing System.Diagnostics;\\nclass Program\\n{\\n    static void Main()\\n    {\\n        Console.WriteLine(42);\\n        Put.WriteRandom();\\n#if !EXPAND_GENERATOR\\n        Console.WriteLine(24);\\n#endif\\n    }\\n}\\n" +
                                "#region Expanded\\nnamespace SampleLibrary { public static class Put { private static readonly Xorshift rnd = new Xorshift(); public static void WriteRandom() => Trace.WriteLine(rnd.Next()); } } \\nnamespace SampleLibrary { public class Xorshift : Random { private uint x = 123456789; private uint y = 362436069; private uint z = 521288629; private uint w; private static readonly Random rnd = new Random(); public Xorshift() : this(rnd.Next()) { } public Xorshift(int seed) { w = (uint)seed; } protected override double Sample() => InternalSample() * (1.0 / uint.MaxValue); private uint InternalSample() { uint t = x ^ (x << 11); x = y; y = z; z = w; return w = (w ^ (w >> 19)) ^ (t ^ (t >> 8)); } } } \\n#endregion Expanded\\n\" } },\n" +
                        "{\"/home/source/Program2.cs\", new SourceCode{ Path=\"/home/source/Program2.cs\", " +
                            "Code=\"using SampleLibrary;\\nusing System;\\nusing System.Diagnostics;\\nclass Program2\\n{\\n    static void Main()\\n    {\\n        Console.WriteLine(42);\\n        Put2.Write();\\n    }\\n}\\n" +
                                "#region Expanded\\nnamespace SampleLibrary { public static class Put2 { public static void Write() => Put.WriteRandom(); } } \\nnamespace SampleLibrary { public static class Put { private static readonly Xorshift rnd = new Xorshift(); public static void WriteRandom() => Trace.WriteLine(rnd.Next()); } } \\nnamespace SampleLibrary { public class Xorshift : Random { private uint x = 123456789; private uint y = 362436069; private uint z = 521288629; private uint w; private static readonly Random rnd = new Random(); public Xorshift() : this(rnd.Next()) { } public Xorshift(int seed) { w = (uint)seed; } protected override double Sample() => InternalSample() * (1.0 / uint.MaxValue); private uint InternalSample() { uint t = x ^ (x << 11); x = y; y = z; z = w; return w = (w ^ (w >> 19)) ^ (t ^ (t >> 8)); } } } \\n#endregion Expanded\\n\" } },\n" +
                    "};\n" +
                "}}\n");

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
                    @"expander version\(\d+\.\d+\.\d+\.\d+\) is older than embedder of OtherDependency\(2147483647\.2147483647\.2147483647\.2147483647\)");

        }
    }
}
