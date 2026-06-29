using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;

namespace SourceExpander;

public abstract class GeneratorTestBase<TSourceGenerator> where TSourceGenerator : new()
{
    public class TestBase : CSharpSourceGeneratorTest<TSourceGenerator>
    {
        public TestBase()
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net100;
            ParseOptions = ParseOptions.WithLanguageVersion(LanguageVersion.Preview);
        }
    }

    protected static Solution CreateOtherReference(
        Solution solution, ProjectId projectId, SourceFileCollection documents,
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

    protected internal virtual ImmutableArray<PackageIdentity> Packages => [];
}
