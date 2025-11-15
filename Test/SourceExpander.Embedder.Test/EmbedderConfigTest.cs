using System;
using System.Collections.Generic;

namespace SourceExpander.Embedder.Test
{
    public class EmbedderConfigTest
    {
        public static IEnumerable<Func<(string[], string[], string, bool)>> IncludeExclude_Data()
        {
            yield return () => (
                [], [],
                "/home/dir1/dir2/Program.cs",
                true
            );
            yield return () => (
                ["/home/dir1/dir2/Program.cs"], [],
                "/home/dir1/dir2/Program.cs",
                true
            );
            yield return () => (
                ["/home/**"], [],
                "/home/dir1/dir2/Program.cs",
                true
            );
            yield return () => (
                ["/home/**/*.cs"], [],
                "/home/dir1/dir2/Program.cs",
                true
            );
            yield return () => (
                ["/home/*/*.cs"], [],
                "/home/dir1/dir2/Program.cs",
                false
            );
            yield return () => (
                ["**/*.cs"], [],
                "/home/dir1/dir2/Program.cs",
                true
            );
            yield return () => (
                ["**"], [],
                "/home/dir1/dir2/Program.cs",
                true
            );
            yield return () => (
                [], ["/home/dir1/dir2/Program.cs"],
                "/home/dir1/dir2/Program.cs",
                false
            );
            yield return () => (
                ["/home/**/*.cs"], ["/home/dir1/dir2/Program.cs"],
                "/home/dir1/dir2/Program.cs",
                false
            );
            yield return () => (
                [], ["**/Program.cs"],
                "/home/dir1/dir2/Program.cs",
                false
            );
            yield return () => (
                [], ["**/dir1**/Program.cs"],
                "/home/dir1/dir2/Program.cs",
                false
            );
            yield return () => (
                [], ["**/*.cs"],
                @"C:\home\dir1\dir2\Program.cs",
                false
            );
            yield return () => (
                [], ["**/home/**/*.cs"],
                @"C:\home\dir1\dir2\Program.cs",
                false
            );
        }

        [Test]
        [MethodDataSource(nameof(IncludeExclude_Data))]
        public void IncludeExclude(string[] include, string[] exclude, string filePath, bool expected)
        {
            new EmbedderConfig(include: include, exclude: exclude).IsMatch(filePath).ShouldBe(expected);
        }
    }
}
