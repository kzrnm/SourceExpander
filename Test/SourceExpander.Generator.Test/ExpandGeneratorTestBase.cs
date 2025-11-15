using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using SourceExpander.Expanded;

namespace SourceExpander
{
    public class ExpandGeneratorTestBase
    {
        public class Test : CSharpSourceGeneratorTest<ExpandGenerator>
        {
            public Test()
            {
                ParseOptions = ParseOptions.WithLanguageVersion(EmbeddedLanguageVersionEnum);
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80.AddPackages(Packages);
            }
        }

        internal static Solution CreateOtherReference(Solution solution,
            ProjectId projectId,
            SourceFileCollection documents,
            string otherName = "Other",
            string otherAssemblyName = "Other",
            CSharpCompilationOptions compilationOptions = null)
        {
            compilationOptions ??= new(OutputKind.DynamicallyLinkedLibrary);

            var targetProject = solution.GetProject(projectId);

            var project = solution.AddProject(otherName, otherAssemblyName, "C#")
                .WithMetadataReferences(targetProject.MetadataReferences)
                .WithCompilationOptions(compilationOptions);
            foreach (var (filename, content) in documents)
            {
                project = project.AddDocument(Path.GetFileNameWithoutExtension(filename), content, filePath: filename).Project;
            }

            return project.Solution.AddProjectReference(projectId, new(project.Id));
        }


        internal static ImmutableArray<PackageIdentity> Packages = [new PackageIdentity("SourceExpander.Core", "2.6.0")];
        public static string ExpanderVersion => typeof(ExpandGenerator).Assembly.GetName().Version.ToString();
        public static readonly LanguageVersion EmbeddedLanguageVersionEnum = LanguageVersion.Preview;

        public static CSharpCompilation CreateCompilation(
            IEnumerable<SyntaxTree> syntaxTrees,
            CSharpCompilationOptions compilationOptions,
            IEnumerable<MetadataReference> additionalMetadatas = null,
            string assemblyName = "TestAssembly")
        {
            additionalMetadatas ??= Array.Empty<MetadataReference>();
            return CSharpCompilation.Create(
                assemblyName: assemblyName,
                syntaxTrees: syntaxTrees,
                references: DefaultMetadatas.Concat(additionalMetadatas),
                options: compilationOptions);
        }

        private static IEnumerable<MetadataReference> DefaultMetadatas { get; } = GetDefaultMetadatas();
        private static IEnumerable<MetadataReference> GetDefaultMetadatas()
        {
            var directory = Path.GetDirectoryName(typeof(object).Assembly.Location);
            foreach (var file in Directory.EnumerateFiles(directory, "System*.dll"))
            {
                yield return MetadataReference.CreateFromFile(file);
            }
        }

        protected static GeneratorResult RunGenerator(
               Compilation compilation,
               ISourceGenerator generator,
               IEnumerable<AdditionalText> additionalTexts = null,
               CSharpParseOptions parseOptions = null,
               AnalyzerConfigOptionsProvider optionsProvider = null)
            => RunGenerator(compilation, new[] { generator }, additionalTexts, parseOptions, optionsProvider);
        protected static GeneratorResult RunGenerator(
               Compilation compilation,
               IEnumerable<ISourceGenerator> generators,
               IEnumerable<AdditionalText> additionalTexts = null,
               CSharpParseOptions parseOptions = null,
               AnalyzerConfigOptionsProvider optionsProvider = null)
        {
            var driver = CSharpGeneratorDriver.Create(generators, additionalTexts, parseOptions, optionsProvider);
            driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

            return new GeneratorResult((CSharpCompilation)outputCompilation, diagnostics,
                outputCompilation.SyntaxTrees.Except(compilation.SyntaxTrees, new SyntaxComparer())
                .Cast<CSharpSyntaxTree>().ToImmutableArray());
        }

        private class SyntaxComparer : IEqualityComparer<SyntaxTree>
        {
            public bool Equals(SyntaxTree x, SyntaxTree y) => x.IsEquivalentTo(y);
            public int GetHashCode(SyntaxTree obj) => obj.FilePath?.GetHashCode() ?? 0;
        }
        protected class GeneratorResult(
         CSharpCompilation outputCompilation,
         ImmutableArray<Diagnostic> diagnostics,
         ImmutableArray<CSharpSyntaxTree> addedSyntaxTrees)
        {
            public CSharpCompilation OutputCompilation { get; } = outputCompilation;
            public ImmutableArray<Diagnostic> Diagnostics { get; } = diagnostics;
            public ImmutableArray<CSharpSyntaxTree> AddedSyntaxTrees { get; } = addedSyntaxTrees;
        }
        public static IReadOnlyDictionary<string, SourceCode> GetExpandedFilesWithCore(Compilation compilation)
            => (IReadOnlyDictionary<string, SourceCode>)GetExpandedFiles(compilation);
        internal static object GetExpandedFiles(Compilation compilation)
        {
            using var ms = new MemoryStream();
            if (!compilation.Emit(ms).Success)
                throw new ArgumentException("compilation is failed", nameof(compilation));
            ms.Position = 0;
            var alc = new AssemblyLoadContext("GetExpandedFiles", true);
            try
            {
                return alc.LoadFromStream(ms)
                    .GetType("SourceExpander.Expanded.ExpandedContainer")
                    .GetProperty("Files").GetValue(null);
            }
            finally
            {
                alc.Unload();
            }
        }

        private static readonly string dir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        public static string GetTestDataPath(params string[] paths)
        {
            var withDir = new string[paths.Length + 2];
            withDir[0] = dir;
            withDir[1] = "testdata";
            Array.Copy(paths, 0, withDir, 2, paths.Length);
            return Path.Combine(withDir);
        }

        private static IEnumerable<string> GetSampleDllPaths()
        {
            yield return GetTestDataPath("SampleLibrary.Old.dll");
            yield return GetTestDataPath("SampleLibrary2.dll");
        }

        protected static readonly IEnumerable<MetadataReference> sampleLibReferences
            = GetSampleDllPaths().Select(path => MetadataReference.CreateFromFile(path));
        protected static readonly MetadataReference coreReference
            = MetadataReference.CreateFromFile(typeof(SourceCode).Assembly.Location);
    }
}
