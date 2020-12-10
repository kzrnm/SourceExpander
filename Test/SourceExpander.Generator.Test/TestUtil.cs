using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.CodeAnalysis;
using SourceExpander.Expanded;

namespace SourceExpander.Generator.Test
{
    internal static partial class TestUtil
    {
        public static IReadOnlyDictionary<string, SourceCode> GetExpandedFilesWithCore(Compilation compilation)
            => (IReadOnlyDictionary<string, SourceCode>)MetadataUtil.GetExpandedFiles(compilation);

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
    }
}
