﻿using System.Collections.Generic;
using System.IO;
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
                references: TestUtil.withCoreReferenceMetadatas,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            );

            var sampleReferences = TestUtil.GetSampleDllPaths().Select(path => MetadataReference.CreateFromFile(path));
            var compilation = CSharpCompilation.Create(
                assemblyName: "TestAssembly",
                syntaxTrees: syntaxTrees,
                references: TestUtil.withCoreReferenceMetadatas.Concat(sampleReferences).Append(newerEmbedderCompilation.ToMetadataReference()),
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
                .Be(File.ReadAllText(TestUtil.GetTestDataPath("wants", "olderversion.test.txt")));

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
