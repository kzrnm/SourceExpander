﻿using System.Collections.Immutable;
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using static SourceExpander.Embedder.TestUtil;

namespace SourceExpander.Embedder.Generate.Test
{
    public class AllowUnsafeTest
    {
        [Fact]
        public void GenerateAllowUnsafeTest()
        {
            var compilation = CSharpCompilation.Create(
                assemblyName: "TestAssembly",
                syntaxTrees: GetTestSyntaxes(),
                references: defaultMetadatas.Append(expanderCoreReference),
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                    .WithAllowUnsafe(true)
                );
            compilation.SyntaxTrees.Should().HaveCount(TestSyntaxesCount);
            compilation.GetDiagnostics().Should().OnlyContain(d => d.Id == "CS8019");

            var generator = new EmbedderGenerator();
            var opts = new CSharpParseOptions(kind: SourceCodeKind.Regular, documentationMode: DocumentationMode.Parse);
            var driver = CSharpGeneratorDriver.Create(new[] { generator }, parseOptions: opts);
            driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);
            diagnostics.Should().BeEmpty();
            outputCompilation.GetDiagnostics().Should().OnlyContain(d => d.Id == "CS8019");
            outputCompilation.SyntaxTrees.Should().HaveCount(TestSyntaxesCount + 1);

            var reporter = new MockDiagnosticReporter();
            new EmbeddingResolver(compilation, opts, reporter).ResolveFiles()
                .Should()
                .BeEquivalentTo(embeddedFiles);
            reporter.Diagnostics.Should().BeEmpty();

            var metadata = outputCompilation.Assembly.GetAttributes()
                .Where(x => x.AttributeClass?.Name == nameof(System.Reflection.AssemblyMetadataAttribute))
                .ToDictionary(x => (string)x.ConstructorArguments[0].Value, x => (string)x.ConstructorArguments[1].Value);
            metadata.Should().NotContainKey("SourceExpander.EmbeddedSourceCode");
            metadata.Should().ContainKey("SourceExpander.EmbeddedSourceCode.GZipBase32768");

            var embedded = metadata["SourceExpander.EmbeddedSourceCode.GZipBase32768"];
            Newtonsoft.Json.JsonConvert.DeserializeObject<SourceFileInfo[]>(
                SourceFileInfoUtil.FromGZipBase32768(embedded))
                .Should()
                .BeEquivalentTo(embeddedFiles);
            System.Text.Json.JsonSerializer.Deserialize<SourceFileInfo[]>(
                SourceFileInfoUtil.FromGZipBase32768(embedded))
                .Should()
                .BeEquivalentTo(embeddedFiles);

            outputCompilation.SyntaxTrees.Should().HaveCount(TestSyntaxesCount + 1);
            diagnostics.Should().BeEmpty();

            outputCompilation.SyntaxTrees
                .Should()
                .ContainSingle(tree => tree.GetRoot(default).ToString().Contains("[assembly: AssemblyMetadataAttribute(\"SourceExpander.EmbeddedSourceCode.GZipBase32768\","))
                .Which
                .ToString()
                .Should()
                .ContainAll(
                "[assembly: AssemblyMetadataAttribute(\"SourceExpander.EmbeddedSourceCode.GZipBase32768\",",
                "[assembly: AssemblyMetadataAttribute(\"SourceExpander.EmbedderVersion\",",
                "[assembly: AssemblyMetadataAttribute(\"SourceExpander.EmbeddedAllowUnsafe\",\"true\")");
        }

        static ImmutableArray<SourceFileInfo> embeddedFiles
            = ImmutableArray.Create(
                new SourceFileInfo
                (
                    "TestAssembly>F/N.cs",
                    new string[] { "Test.F.N" },
                    new string[] { "using System;", "using System.Diagnostics;", "using static System.Console;" },
                    new string[] { "TestAssembly>F/NumType.cs", "TestAssembly>Put.cs" },
                    "namespace Test.F{class N{public static void WriteN(){Console.Write(NumType.Zero);Write(\"N\");Trace.Write(\"N\");Put.Nested.Write(\"N\");}}}"
                ), new SourceFileInfo
                (
                    "TestAssembly>F/NumType.cs",
                    new string[] { "Test.F.NumType" },
                    ImmutableArray.Create<string>(),
                    ImmutableArray.Create<string>(),
                    "namespace Test.F{public enum NumType{Zero,Pos,Neg,}}"
                ), new SourceFileInfo
                (
                    "TestAssembly>I/D.cs",
                    new string[] { "Test.I.IntRecord", "Test.I.D<T>" },
                    new string[] { "using System.Diagnostics;", "using System;", "using System.Collections.Generic;" },
                    new string[] { "TestAssembly>Put.cs" },
                    "namespace Test.I{using System.Collections;public record IntRecord(int n);class D<T> : IComparer<T>{public int Compare(T x, T y) => throw new NotImplementedException();public static void WriteType(){Console.Write(typeof(T).FullName);Trace.Write(typeof(T).FullName);Put.Nested.Write(typeof(T).FullName);}}}"
                ), new SourceFileInfo
                (
                    "TestAssembly>Put.cs",
                    new string[] { "Test.Put", "Test.Put.Nested" },
                    new string[] { "using System.Diagnostics;" },
                    ImmutableArray.Create<string>(),
                    "namespace Test{static class Put{public class Nested{public static void Write(string v){Debug.WriteLine(v);}}}}"
                ));
    }
}
