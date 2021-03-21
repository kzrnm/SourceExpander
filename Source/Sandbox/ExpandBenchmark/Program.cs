using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.CsProj;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Diagnostics.Tracing.Parsers.IIS_Trace;
using SourceExpander;

#if DEBUG
BenchmarkSwitcher.FromAssembly(typeof(BenchmarkConfig).Assembly).Run(args, new DebugInProcessConfig());
return;
#else
_ = BenchmarkRunner.Run(typeof(Benchmark).Assembly);
#endif
public class BenchmarkConfig : ManualConfig
{
    public BenchmarkConfig()
    {
        //AddDiagnoser(MemoryDiagnoser.Default);
        AddExporter(BenchmarkDotNet.Exporters.MarkdownExporter.GitHub);
        AddJob(Job.ShortRun.WithToolchain(CsProjCoreToolchain.NetCoreApp50));
        WithOption(ConfigOptions.DisableOptimizationsValidator, true);
    }
}

static class TestUtil
{
    public static CSharpCompilation CreateCompilation(
        IEnumerable<SyntaxTree> syntaxTrees,
        CSharpCompilationOptions compilationOptions = null,
        string assemblyName = "TestProject")
    {
        compilationOptions ??= new(OutputKind.DynamicallyLinkedLibrary);
        return CSharpCompilation.Create(
            assemblyName: assemblyName,
            syntaxTrees: syntaxTrees,
            references: references,
            options: compilationOptions);
    }
    public static readonly ImmutableArray<MetadataReference> references = ReferenceAssemblies.Net.Net50
            .AddPackages(ImmutableArray.Create(
                new PackageIdentity("Competitive.IO", "0.5.1"),
                new PackageIdentity("ac-library-csharp", "1.4.4")))
            .ResolveAsync(null, CancellationToken.None).Result;

    public static (Compilation outputCompilation, ImmutableArray<Diagnostic> diagnostics) RunGenerator(
           Compilation compilation,
           ISourceGenerator generator,
           IEnumerable<AdditionalText> additionalTexts = null,
           CSharpParseOptions parseOptions = null,
           AnalyzerConfigOptionsProvider optionsProvider = null,
           CancellationToken cancellationToken = default)
        => RunGenerator(compilation, new[] { generator }, additionalTexts, parseOptions, optionsProvider, cancellationToken);
    public static (Compilation outputCompilation, ImmutableArray<Diagnostic> diagnostics) RunGenerator(
           Compilation compilation,
           IEnumerable<ISourceGenerator> generators,
           IEnumerable<AdditionalText> additionalTexts = null,
           CSharpParseOptions parseOptions = null,
           AnalyzerConfigOptionsProvider optionsProvider = null,
           CancellationToken cancellationToken = default)
    {
        var driver = CSharpGeneratorDriver.Create(generators, additionalTexts, parseOptions, optionsProvider);
        driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics, cancellationToken);
        return (outputCompilation, diagnostics);
    }
}

public class InMemoryAdditionalText : AdditionalText
{
    public InMemoryAdditionalText(string path, string source) : this(path, source, new UTF8Encoding(false)) { }
    public InMemoryAdditionalText(string path, string source, Encoding encoding)
    {
        Path = path;
        sourceText = SourceText.From(source, encoding);
    }
    public override string Path { get; }
    private readonly SourceText sourceText;
    public override SourceText GetText(CancellationToken cancellationToken = default) => sourceText;
}

[Config(typeof(BenchmarkConfig))]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class Benchmark
{
    static string CurrentPath([CallerFilePath] string path = "") => path;
    readonly CSharpCompilation compilation;
    public Benchmark()
    {
        compilation = TestUtil.CreateCompilation(new SyntaxTree[] {
            CSharpSyntaxTree.ParseText(@"
using System;
using System.Linq;
using AtCoder;
using Kzrnm.Competitive.IO;

namespace Solvers
{
    public class ConvolutionSolver
    {
        public void Solve()
        {
            var cr = new PropertyConsoleReader();
            int n = cr;
            int m = cr;
            int[] a = Console.ReadLine().Split(' ').Select(int.Parse).ToArray();
            int[] b = Console.ReadLine().Split(' ').Select(int.Parse).ToArray();

            Console.WriteLine(string.Join(' ', MathLib.Convolution<Mod998244353>(a, b)));
        }
    }
}"),
        });
    }

    static ImmutableArray<AdditionalText> CreateConfig(string json)
        => ImmutableArray.Create<AdditionalText>(new InMemoryAdditionalText("/foo/bar/SourceExpander.Embedder.Config.json", json));

    const int cancellationMilliseconds = 10000;

    [Benchmark]
    public Compilation Default()
    {
        var cts = new CancellationTokenSource(cancellationMilliseconds);
        var (outCompilation, diag) = TestUtil.RunGenerator(
            compilation, new ExpandGenerator(), CreateConfig(@"
{
    ""enable-minify"": true
}
"), cancellationToken: cts.Token);
        return outCompilation;
    }
}
