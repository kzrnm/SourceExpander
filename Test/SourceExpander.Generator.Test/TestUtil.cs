using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

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

        public static readonly MetadataReference[] defaultMetadatas = GetDefaulMetadatas().ToArray();
        public static IEnumerable<MetadataReference> GetDefaulMetadatas()
        {
            var directory = Path.GetDirectoryName(typeof(object).Assembly.Location);
            foreach (var file in Directory.EnumerateFiles(directory, "System*.dll"))
            {
                yield return MetadataReference.CreateFromFile(file);
            }
        }
    }
}
