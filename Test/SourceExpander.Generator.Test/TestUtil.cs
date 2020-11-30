using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using SourceExpander.Expanded;

namespace SourceExpander.Generator.Test
{
    internal static class TestUtil
    {
        public static object GetExpandedFiles(Compilation compilation)
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
        public static IReadOnlyDictionary<string, SourceCode> GetExpandedFilesWithCore(Compilation compilation)
            => (IReadOnlyDictionary<string, SourceCode>)GetExpandedFiles(compilation);

        private static readonly string dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static string GetTestDataPath(params string[] paths)
        {
            var withDir = new string[paths.Length + 2];
            withDir[0] = dir;
            withDir[1] = "testdata";
            Array.Copy(paths, 0, withDir, 2, paths.Length);
            return Path.Combine(withDir);
        }

        public static IEnumerable<string> GetSampleDllPaths()
        {
            yield return GetTestDataPath("SampleLibrary.Old.dll");
            yield return GetTestDataPath("SampleLibrary2.dll");
        }

        private static readonly MetadataReference coreReference
            = MetadataReference.CreateFromFile(typeof(SourceCode).Assembly.Location);
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
