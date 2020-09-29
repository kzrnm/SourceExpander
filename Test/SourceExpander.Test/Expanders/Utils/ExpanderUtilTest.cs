using FluentAssertions;
using Xunit;

namespace SourceExpander.Expanders.Utils
{
    public class ExpanderUtilTest
    {
        public static TheoryData ToLinesData = new TheoryData<string, string[]>
        {
            {
                "abc\r\ndef\nghi\rjkl\nmnopq\r\n\r\nrstuvwxyz",
                new string[]{ "abc","def","ghi","jkl","mnopq","","rstuvwxyz" }
            }
        };

        [Theory]
        [MemberData(nameof(ToLinesData))]
        public void ToLinesTest(string input, string[] expected)
        {
            ExpanderUtil.ToLines(input).Should().Equal(expected);
        }


        public static TheoryData ToSimpleClassNameData = new TheoryData<string, string>
        {
            {
                "System.Console",
                "Console"
            },
            {
                "System.Collections.Generic.List<int>",
                "List"
            },
            {
                "Global",
                "Global"
            },
        };
        [Theory]
        [MemberData(nameof(ToSimpleClassNameData))]
        public void ToSimpleClassNameTest(string input, string expected)
        {

            ExpanderUtil.ToSimpleClassName(input).Should().Be(expected);
        }
    }
}
