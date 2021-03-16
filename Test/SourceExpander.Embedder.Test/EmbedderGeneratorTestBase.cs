using System.Collections.Immutable;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;

namespace SourceExpander.Embedder
{
    public class EmbedderGeneratorTestBase
    {
        public class Test : CSharpSourceGeneratorTest<EmbedderGenerator>
        {
            public Test()
            {
                ParseOptions = ParseOptions.WithLanguageVersion(EmbeddedLanguageVersionEnum);
                ReferenceAssemblies = ReferenceAssemblies.Net.Net50.AddPackages(Packages);
                foreach (var (hintName, sourceText) in CompileTimeTypeMaker.Sources)
                {
                    TestState.GeneratedSources.Add((typeof(EmbedderGenerator), hintName, sourceText));
                }
            }
        }

        internal static Solution CreateOtherReference(Solution solution,
            ProjectId projectId,
            SourceFileCollection documents,
            CSharpCompilationOptions compilationOptions = null)
        {
            if (compilationOptions is null)
                compilationOptions = new(OutputKind.DynamicallyLinkedLibrary);

            var targetProject = solution.GetProject(projectId);

            var project = solution.AddProject("Other", "Other", "C#")
                .WithMetadataReferences(targetProject.MetadataReferences)
                .WithCompilationOptions(compilationOptions);
            foreach (var (filename, content) in documents)
            {
                project = project.AddDocument(Path.GetFileNameWithoutExtension(filename), content, filePath: filename).Project;
            }

            return project.Solution.AddProjectReference(projectId, new(project.Id));
        }

        internal static ImmutableArray<PackageIdentity> Packages
            = ImmutableArray.Create(new PackageIdentity("SourceExpander.Core", "2.6.0"));

        public static string EmbedderVersion => typeof(EmbedderGenerator).Assembly.GetName().Version.ToString();
        private static readonly LanguageVersion EmbeddedLanguageVersionEnum = LanguageVersion.CSharp9;
        public static string EmbeddedLanguageVersion => "9.0";

        public static InMemorySourceText enableMinifyJson = new(
            "/foo/bar/SourceExpander.Embedder.Config.json", @"
{
    ""$schema"": ""https://raw.githubusercontent.com/naminodarie/SourceExpander/master/schema/embedder.schema.json"",
    ""embedding-type"": ""Raw"",
    ""enable-minify"": true
}
");
    }
}
