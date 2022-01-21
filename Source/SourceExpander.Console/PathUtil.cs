using System;
using System.IO;
using System.Linq;

namespace SourceExpander
{
    /// <summary>
    /// Path Utility
    /// </summary>
    internal static class PathUtil
    {
        /// <summary>
        /// Find project that contains <paramref name="filePath"/>.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Not found <paramref name="filePath"/>.</exception>
        /// <exception cref="FileNotFoundException">Not found project that contains <paramref name="filePath"/>.</exception>
        public static string GetProjectPath(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            if (!fileInfo.Exists)
                throw new ArgumentException($"{filePath} is not found.", nameof(filePath));

            for (var directory = fileInfo.Directory; directory is not null; directory = directory.Parent)
            {
                if (directory.EnumerateFiles("*.csproj").FirstOrDefault() is { } projFile)
                    return projFile.FullName;
            }
            throw new FileNotFoundException($"Not found project that contains {filePath}");
        }
    }
}
