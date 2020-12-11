using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using SourceExpander.Expanded;

namespace SourceExpander.Generator
{
    public class ExpandGeneratorTestBase : GeneratorTestBase
    {
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

        private static readonly string dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
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
