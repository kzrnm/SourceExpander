using System.Runtime.CompilerServices;

namespace SourceExpander;

public class PathUtilTest
{
    private static string GetFilePath([CallerFilePath] string filePath = "") => filePath;
    [Test]
    public async Task GetProjectPath_FileNotFound()
    {
        var currentFilePath = GetFilePath();
        await new Action(() => PathUtil.GetProjectPath(currentFilePath + ".notfound"))
            .Should()
            .Throw<ArgumentException>()
            .HaveMessage($"{currentFilePath + ".notfound"} is not found. (Parameter 'filePath')");
    }
    [Test]
    public async Task GetProjectPath_ProjectNotFound()
    {
        var fileInfo = new FileInfo(GetFilePath()).Directory!.Parent!.EnumerateDirectories("Utils").Single().EnumerateFiles("EnvironmentUtil.cs").Single();
        await new Action(() => PathUtil.GetProjectPath(fileInfo.FullName))
            .Should()
            .Throw<FileNotFoundException>()
            .HaveMessage($"Not found project that contains {fileInfo.FullName}");
    }
    [Test]
    public async Task GetProjectPath_Relative()
    {
        var currentFullPath = GetFilePath();
        var currentFilePath = Path.GetRelativePath(Environment.CurrentDirectory, currentFullPath);
        await PathUtil.GetProjectPath(currentFilePath).Should().BeEqualTo(currentFullPath.Replace("PathUtilTest.cs", "SourceExpander.Console.Test.csproj"));
    }
    [Test]
    public async Task GetProjectPath()
    {
        var currentFilePath = GetFilePath();
        await PathUtil.GetProjectPath(currentFilePath).Should().BeEqualTo(currentFilePath.Replace("PathUtilTest.cs", "SourceExpander.Console.Test.csproj"));
    }
}
