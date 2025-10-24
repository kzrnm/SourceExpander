namespace SourceExpander.Embedder.Test
{
    public class EmbedderConfigTest
    {
        public static TheoryData<string[], string[], string, bool> IncludeExclude_Data = new()
        {
            {
                [], [],
                "/home/dir1/dir2/Program.cs",
                true
            },
            {
                ["/home/dir1/dir2/Program.cs"], [],
                "/home/dir1/dir2/Program.cs",
                true
            },
            {
                ["/home/**"], [],
                "/home/dir1/dir2/Program.cs",
                true
            },
            {
                ["/home/**/*.cs"], [],
                "/home/dir1/dir2/Program.cs",
                true
            },
            {
                ["/home/*/*.cs"], [],
                "/home/dir1/dir2/Program.cs",
                false
            },
            {
                ["**/*.cs"], [],
                "/home/dir1/dir2/Program.cs",
                true
            },
            {
                ["**"], [],
                "/home/dir1/dir2/Program.cs",
                true
            },

            {
                [], ["/home/dir1/dir2/Program.cs"],
                "/home/dir1/dir2/Program.cs",
                false
            },
            {
                ["/home/**/*.cs"], ["/home/dir1/dir2/Program.cs"],
                "/home/dir1/dir2/Program.cs",
                false
            },
            {
                [], ["**/Program.cs"],
                "/home/dir1/dir2/Program.cs",
                false
            },
            {
                [], ["**/dir1**/Program.cs"],
                "/home/dir1/dir2/Program.cs",
                false
            },

            {
                [], ["**/*.cs"],
                @"C:\home\dir1\dir2\Program.cs",
                false
            },
            {
                [], ["**/home/**/*.cs"],
                @"C:\home\dir1\dir2\Program.cs",
                false
            },
        };

        [Theory]
        [MemberData(nameof(IncludeExclude_Data))]
        public void IncludeExclude(string[] include, string[] exclude, string filePath, bool expected)
        {
            new EmbedderConfig(include: include, exclude: exclude).IsMatch(filePath).ShouldBe(expected);
        }
    }
}
