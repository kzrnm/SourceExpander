using System;
using System.Collections.Immutable;
using System.IO;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;

namespace SourceExpander.Embedder
{
    public class EmbeddingGeneratorTestBase : GeneratorTestBase
    {
        public class Test : CSharpSourceGeneratorTest<EmbedderGenerator>
        {
            public Test()
            {
                ReferenceAssemblies = ReferenceAssemblies.AddPackages(Packages);
            }
        }
        internal static ImmutableArray<PackageIdentity> Packages
            = ImmutableArray.Create(new PackageIdentity("SourceExpander.Core", "2.6.0"));


        public static readonly MetadataReference expanderCoreReference = MetadataReference.CreateFromFile(typeof(SourceFileInfo).Assembly.Location);

        public static InMemoryAdditionalText enableMinifyJson = new(
            "/foo/bar/SourceExpander.Embedder.Config.json", @"
{
    ""$schema"": ""https://raw.githubusercontent.com/naminodarie/SourceExpander/master/schema/embedder.schema.json"",
    ""enable-minify"": true
}
");
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
    }
}
