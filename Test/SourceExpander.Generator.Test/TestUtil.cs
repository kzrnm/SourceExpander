using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;

namespace SourceExpander.Generator.Test
{
    internal class TestUtil
    {
        public static IEnumerable<string> GetSampleDllPaths()
        {
            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            yield return Path.Combine(dir, "testdata", "SampleLibrary.Old.dll");
            yield return Path.Combine(dir, "testdata", "SampleLibrary2.dll");
        }

        private static readonly MetadataReference coreReference
            = MetadataReference.CreateFromFile(typeof(Expanded.SourceCode).Assembly.Location);
        public static readonly MetadataReference[] noCoreReferenceMetadatas = GetDefaulMetadatas().ToArray();
        public static readonly MetadataReference[] withCoreReferenceMetadatas
            = noCoreReferenceMetadatas.Append(coreReference).ToArray();

        private static IEnumerable<MetadataReference> GetDefaulMetadatas()
        {
            var directory = Path.GetDirectoryName(typeof(object).Assembly.Location);
            foreach (var file in Directory.EnumerateFiles(directory, "System*.dll"))
            {
                yield return MetadataReference.CreateFromFile(file);
            }
        }
    }
}
