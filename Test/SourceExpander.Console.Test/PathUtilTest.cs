using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using FluentAssertions;
using Xunit;

namespace SourceExpander
{
    public class PathUtilTest
    {
        private static string GetFilePath([CallerFilePath] string filePath = "") => filePath;
        [Fact]
        public void GetProjectPath_FileNotFound()
        {
            var currentFilePath = GetFilePath();
            new Action(() => PathUtil.GetProjectPath(currentFilePath + ".notfound"))
                .Should().ThrowExactly<ArgumentException>()
                .WithMessage($"{currentFilePath + ".notfound"} is not found. (Parameter 'filePath')")
                .WithParameterName("filePath");
        }
        [Fact]
        public void GetProjectPath_ProjectNotFound()
        {
            var fileInfo = new FileInfo(GetFilePath()).Directory!.Parent!.EnumerateDirectories("Utils").Single().EnumerateFiles("EnvironmentUtil.cs").Single();
            new Action(() => PathUtil.GetProjectPath(fileInfo.FullName))
                .Should().ThrowExactly<FileNotFoundException>()
                .WithMessage($"Not found project that contains {fileInfo.FullName}");
        }
        [Fact]
        public void GetProjectPath_Relative()
        {
            var currentFullPath = GetFilePath();
            var currentFilePath = Path.GetRelativePath(Environment.CurrentDirectory, currentFullPath);
            PathUtil.GetProjectPath(currentFilePath).Should().Be(currentFullPath.Replace("PathUtilTest.cs", "SourceExpander.Console.Test.csproj"));
        }
        [Fact]
        public void GetProjectPath()
        {
            var currentFilePath = GetFilePath();
            PathUtil.GetProjectPath(currentFilePath).Should().Be(currentFilePath.Replace("PathUtilTest.cs", "SourceExpander.Console.Test.csproj"));
        }
    }
}
