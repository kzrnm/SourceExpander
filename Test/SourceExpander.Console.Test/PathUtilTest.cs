using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace SourceExpander;

public class PathUtilTest
{
    private static string GetFilePath([CallerFilePath] string filePath = "") => filePath;
    [Test]
    public void GetProjectPath_FileNotFound()
    {
        var currentFilePath = GetFilePath();
        new Action(() => PathUtil.GetProjectPath(currentFilePath + ".notfound"))
            .ShouldThrow<ArgumentException>()
            .ShouldSatisfyAllConditions([
                e => e.Message.ShouldBe($"{currentFilePath + ".notfound"} is not found. (Parameter 'filePath')"),
                e => e.ParamName.ShouldBe("filePath"),
            ]);
    }
    [Test]
    public void GetProjectPath_ProjectNotFound()
    {
        var fileInfo = new FileInfo(GetFilePath()).Directory!.Parent!.EnumerateDirectories("Utils").Single().EnumerateFiles("EnvironmentUtil.cs").Single();
        new Action(() => PathUtil.GetProjectPath(fileInfo.FullName))
            .ShouldThrow<FileNotFoundException>()
            .ShouldSatisfyAllConditions([
                e => e.Message.ShouldBe($"Not found project that contains {fileInfo.FullName}"),
            ]);
    }
    [Test]
    public void GetProjectPath_Relative()
    {
        var currentFullPath = GetFilePath();
        var currentFilePath = Path.GetRelativePath(Environment.CurrentDirectory, currentFullPath);
        PathUtil.GetProjectPath(currentFilePath).ShouldBe(currentFullPath.Replace("PathUtilTest.cs", "SourceExpander.Console.Test.csproj"));
    }
    [Test]
    public void GetProjectPath()
    {
        var currentFilePath = GetFilePath();
        PathUtil.GetProjectPath(currentFilePath).ShouldBe(currentFilePath.Replace("PathUtilTest.cs", "SourceExpander.Console.Test.csproj"));
    }
}
