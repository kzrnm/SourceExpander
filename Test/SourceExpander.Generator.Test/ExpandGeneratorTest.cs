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
            var sampleReference = MetadataReference.CreateFromFile(GetSampleDllPath());
            var compilation = CSharpCompilation.Create(
                assemblyName: "TestAssembly",
                syntaxTrees: GetSyntaxes(),
                references: defaultMetadatas.Append(sampleReference),
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            compilation.SyntaxTrees.Should().HaveCount(TestSyntaxes.Length);
            compilation.GetDiagnostics().Should().BeEmpty();

            var generator = new ExpandGenerator();
            var driver = CSharpGeneratorDriver.Create(new[] { generator }, parseOptions: new CSharpParseOptions(kind: SourceCodeKind.Regular, documentationMode: DocumentationMode.Parse));
            driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);
            diagnostics.Should().BeEmpty();
            outputCompilation.SyntaxTrees.Should().HaveCount(TestSyntaxes.Length + 1);
        }

        static string GetSampleDllPath()
            => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "SampleLibrary.dll");

        static readonly  SyntaxTree[] TestSyntaxes = GetSyntaxes().ToArray();
        static IEnumerable<SyntaxTree> GetSyntaxes()
        {
            yield return CSharpSyntaxTree.ParseText(
                @"using System;
using SampleLibrary;

class Program
{
    static void Main()
    {
        Console.WriteLine(42);
        Put.WriteRandom();
    }
}",
                path: "/home/source/Program.cs");
            yield return CSharpSyntaxTree.ParseText(
                @"using System;
using SampleLibrary;

class Program2
{
    static void Main()
    {
        Console.WriteLine(42);
        Put.WriteRandom();
    }
}",
                path: "/home/source/Program.cs");
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
