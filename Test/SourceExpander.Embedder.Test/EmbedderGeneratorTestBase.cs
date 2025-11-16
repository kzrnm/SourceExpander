using System.Collections.Immutable;
using System.IO;
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
                ParseOptions = ParseOptions.WithLanguageVersion(EmbeddedLanguageVersionEnum);
                ReferenceAssemblies = new ReferenceAssemblies(
                        "net8.0",
                        new PackageIdentity(
                            "Microsoft.NETCore.App.Ref",
                            "8.0.0"),
                        Path.Combine("ref", "net8.0")).AddPackages(Packages);
                foreach (var (hintName, sourceText) in CompileTimeTypeMaker.Sources)
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
            = [new PackageIdentity("SourceExpander.Core", "2.6.0")];

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
