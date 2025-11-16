#nullable disable
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

namespace SourceExpander
{
    public class CSharpSourceGeneratorTest<TSourceGenerator> : CSharpSourceGeneratorTest<TSourceGenerator, DefaultVerifier>
        where TSourceGenerator : new()
    {
        protected override async Task<(Compilation compilation, ImmutableArray<Diagnostic> generatorDiagnostics)> GetProjectCompilationAsync(Project project, IVerifier verifier, CancellationToken cancellationToken)
        {
            if (project.Name == DefaultTestProjectName)
                return await base.GetProjectCompilationAsync(project, verifier, cancellationToken);
            return (await project.GetCompilationAsync(cancellationToken).ConfigureAwait(false), []);
        }

        public CSharpCompilationOptions CompilationOptions { get; set; } = new(OutputKind.DynamicallyLinkedLibrary);
        protected override CompilationOptions CreateCompilationOptions() => CompilationOptions;
        public CSharpParseOptions ParseOptions { get; set; } = new(languageVersion: LanguageVersion.CSharp9, kind: SourceCodeKind.Regular, documentationMode: DocumentationMode.Parse);
        protected override ParseOptions CreateParseOptions() => ParseOptions;

        public DummyAnalyzerConfigOptions AnalyzerConfigOptions { get; }
            = new();
        protected override AnalyzerOptions GetAnalyzerOptions(Project project) => AnalyzerConfigOptions switch
        {
            { IsEmpty: false } => new AnalyzerOptions(
                project.AnalyzerOptions.AdditionalFiles, AnalyzerConfigOptions.ToProvider()),
            _ => base.GetAnalyzerOptions(project),
        };
    }


    public class DummyAnalyzerConfigOptions : AnalyzerConfigOptions, IEnumerable<KeyValuePair<string, string>>
    {
        public void Add(string key, string value) => dict.Add(key, value);
        public void Add(IEnumerable<KeyValuePair<string, string>> other)
        {
            foreach (var (k, v) in other)
                dict.Add(k, v);
        }

        public bool IsEmpty => dict.Count == 0;
        private readonly Dictionary<string, string> dict = new(KeyComparer);
        public override bool TryGetValue(string key, out string value) => dict.TryGetValue(key, out value);
        public IEnumerator<KeyValuePair<string, string>> GetEnumerator() => ((IEnumerable<KeyValuePair<string, string>>)dict).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)dict).GetEnumerator();

        public AnalyzerConfigOptionsProvider ToProvider() => new DummyAnalyzerConfigOptionsProvider(this);

        private class DummyAnalyzerConfigOptionsProvider(AnalyzerConfigOptions impl) : AnalyzerConfigOptionsProvider
        {
            public override AnalyzerConfigOptions GlobalOptions => impl;
            public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => impl;
            public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) => impl;
        }
    }
}
