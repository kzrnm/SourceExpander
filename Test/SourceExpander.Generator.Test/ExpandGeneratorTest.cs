using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace SourceExpander.Generator.Test
{
    public class ExpandGeneratorTest
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
#end if
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

            var sampleReferences = GetSampleDllPaths().Select(path => MetadataReference.CreateFromFile(path));
            var compilation = CSharpCompilation.Create(
                assemblyName: "TestAssembly",
                syntaxTrees: syntaxTrees,
                references: defaultMetadatas.Concat(sampleReferences),
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithSpecificDiagnosticOptions(new Dictionary<string, ReportDiagnostic> {
                    { "CS8019", ReportDiagnostic.Suppress },
                }));
            compilation.SyntaxTrees.Should().HaveCount(syntaxTrees.Length);

            var generator = new ExpandGenerator();
            var driver = CSharpGeneratorDriver.Create(new[] { generator }, parseOptions: new CSharpParseOptions(kind: SourceCodeKind.Regular, documentationMode: DocumentationMode.Parse));
            driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);
            diagnostics.Should().BeEmpty();
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
                            "Code=\"using SampleLibrary;\\nusing System;\\nusing System.Diagnostics;\\nclass Program\\n{\\n    static void Main()\\n    {\\n        Console.WriteLine(42);\\n        Put.WriteRandom();\\n#if !EXPAND_GENERATOR\\n        Console.WriteLine(24);\\n#end if\\n    }\\n}\\n" +
                                "#region Expanded\\nnamespace SampleLibrary { public static class Put { private static readonly Xorshift rnd = new Xorshift(); public static void WriteRandom() => Trace.WriteLine(rnd.Next()); } } \\nnamespace SampleLibrary { public class Xorshift : Random { private uint x = 123456789; private uint y = 362436069; private uint z = 521288629; private uint w; private static readonly Random rnd = new Random(); public Xorshift() : this(rnd.Next()) { } public Xorshift(int seed) { w = (uint)seed; } protected override double Sample() => InternalSample() * (1.0 / uint.MaxValue); private uint InternalSample() { uint t = x ^ (x << 11); x = y; y = z; z = w; return w = (w ^ (w >> 19)) ^ (t ^ (t >> 8)); } } } \\n#endregion Expanded\" } },\n" +
                        "{\"/home/source/Program2.cs\", new SourceCode{ Path=\"/home/source/Program2.cs\", " +
                            "Code=\"using SampleLibrary;\\nusing System;\\nusing System.Diagnostics;\\nclass Program2\\n{\\n    static void Main()\\n    {\\n        Console.WriteLine(42);\\n        Put2.Write();\\n    }\\n}\\n" +
                                "#region Expanded\\nnamespace SampleLibrary { public static class Put2 { public static void Write() => Put.WriteRandom(); } } \\nnamespace SampleLibrary { public static class Put { private static readonly Xorshift rnd = new Xorshift(); public static void WriteRandom() => Trace.WriteLine(rnd.Next()); } } \\nnamespace SampleLibrary { public class Xorshift : Random { private uint x = 123456789; private uint y = 362436069; private uint z = 521288629; private uint w; private static readonly Random rnd = new Random(); public Xorshift() : this(rnd.Next()) { } public Xorshift(int seed) { w = (uint)seed; } protected override double Sample() => InternalSample() * (1.0 / uint.MaxValue); private uint InternalSample() { uint t = x ^ (x << 11); x = y; y = z; z = w; return w = (w ^ (w >> 19)) ^ (t ^ (t >> 8)); } } } \\n#endregion Expanded\" } },\n" +
                    "};\n" +
                "}}\n");
        }
        [Fact]
        public void GenerateNotFoundTest()
        {
            const string code = @"using System;
using System.Reflection;
using static System.Math;

class Program
{
    static void Main()
    {
        Console.WriteLine(42);
    }
}";
            var syntaxTrees = new[]
            {
                CSharpSyntaxTree.ParseText(
                    code,
                    options: new CSharpParseOptions(documentationMode:DocumentationMode.None),
                    path: "/home/source/Program.cs"),
            };

            var compilation = CSharpCompilation.Create(
                assemblyName: "TestAssembly",
                syntaxTrees: syntaxTrees,
                references: defaultMetadatas,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithSpecificDiagnosticOptions(new Dictionary<string, ReportDiagnostic> {
                    { "CS8019", ReportDiagnostic.Suppress },
                }));
            compilation.SyntaxTrees.Should().HaveCount(syntaxTrees.Length);

            var generator = new ExpandGenerator();
            var driver = CSharpGeneratorDriver.Create(new[] { generator }, parseOptions: new CSharpParseOptions(kind: SourceCodeKind.Regular, documentationMode: DocumentationMode.Parse));
            driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);
            diagnostics.Should().ContainSingle()
                .Which
                .Id
                .Should().Be("EXPAND0001");
            outputCompilation.SyntaxTrees.Should().HaveCount(syntaxTrees.Length + 1);

            outputCompilation.SyntaxTrees
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
                    "{\"/home/source/Program.cs\", new SourceCode{ Path=\"/home/source/Program.cs\", Code=\"using System;\\n" +
                    "class Program\\n{\\n    static void Main()\\n    {\\n        Console.WriteLine(42);\\n    }\\n}\\n#region Expanded\\n#endregion Expanded\" } },\n" +
                "};\n" +
            "}}\n");
        }

        static IEnumerable<string> GetSampleDllPaths()
        {
            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            yield return Path.Combine(dir, "testdata", "SampleLibrary.Old.dll");
            yield return Path.Combine(dir, "testdata", "SampleLibrary2.dll");
        }

        static readonly MetadataReference[] defaultMetadatas = GetDefaulMetadatas().ToArray();
        static IEnumerable<MetadataReference> GetDefaulMetadatas()
        {
            var directory = Path.GetDirectoryName(typeof(object).Assembly.Location);
            foreach (var file in Directory.EnumerateFiles(directory, "System*.dll"))
            {
                yield return MetadataReference.CreateFromFile(file);
            }
        }
    }
}
