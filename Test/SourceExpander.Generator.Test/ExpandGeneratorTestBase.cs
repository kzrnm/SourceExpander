using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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
                ReferenceAssemblies = ReferenceAssemblies.Net.Net100.AddPackages(Packages);
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
