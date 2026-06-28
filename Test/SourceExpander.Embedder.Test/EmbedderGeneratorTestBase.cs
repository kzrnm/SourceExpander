using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;

namespace SourceExpander
{
    public class EmbedderGeneratorTestBase
    {
        public class Test : CSharpSourceGeneratorTest<EmbedderGenerator>
        {
            public Test()
            {
                AnalyzerConfigOptions.Add(
                    typeof(EmbedderConfig.Builder).GetProperties()
                        .Where(p => p.Name is not "SourceText")
                        .Select(p => KeyValuePair.Create($"build_property.SourceExpander_Embedder_{p.Name}", "")));
                ParseOptions = ParseOptions.WithLanguageVersion(EmbeddedLanguageVersionEnum);
                ReferenceAssemblies = ReferenceAssemblies.Net.Net100.AddPackages(Packages);
                foreach (var (hintName, sourceText) in Constants.CompileTimeSources)
                {
                    TestState.GeneratedSources.Add((typeof(EmbedderGenerator), hintName, sourceText));
                }
            }
        }

        internal static Solution CreateOtherReference(Solution solution,
            ProjectId projectId,
            SourceFileCollection documents)
        {
            var targetProject = solution.GetProject(projectId);

            var project = solution.AddProject("Other", "Other", "C#")
                .WithMetadataReferences(targetProject.MetadataReferences)
                .WithCompilationOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            foreach (var (filename, content) in documents)
            {
                project = project.AddDocument(Path.GetFileNameWithoutExtension(filename), content, filePath: filename).Project;
            }

            return project.Solution.AddProjectReference(projectId, new(project.Id));
        }

        internal static ImmutableArray<PackageIdentity> Packages
            = [];

        public static string EmbedderVersion => typeof(EmbedderGenerator).Assembly.GetName().Version.ToString();
        public static readonly LanguageVersion EmbeddedLanguageVersionEnum = LanguageVersion.Preview;
        public static string EmbeddedLanguageVersion => "preview";

        public static InMemorySourceText enableMinifyJson = new(
            "/foo/bar/SourceExpander.Embedder.Config.json", """
{
    "$schema": "https://raw.githubusercontent.com/kzrnm/SourceExpander/master/schema/embedder.schema.json",
    "embedding-type": "Raw",
    "minify-level": "full"
}
""");
    }
}
