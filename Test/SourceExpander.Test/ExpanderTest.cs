using System;
using System.Collections.Generic;
using System.ComponentModel;
using FluentAssertions;
using SourceExpander.Expanders;
using Xunit;

namespace SourceExpander.Test
{
    public class ExpanderTest
    {
        [Fact]
        public void CreateFailTest()
        {
            Expander.Create("", ExpandMethod.All).Should().BeOfType<AllExpander>();
            Expander.Create("", ExpandMethod.NameSyntax).Should().BeOfType<SimpleMatchExpander>();
            Expander.Create("", ExpandMethod.Strict).Should().BeOfType<CompilationExpander>();
            Assert.Throws<InvalidEnumArgumentException>("expandMethod", () => Expander.Create("", (ExpandMethod)(0b10001 << 20)));
        }

        [Fact]
        public void ExpandedTest()
        {
            var expander = new TestExpander("", new SourceFileContainer(Array.Empty<SourceFileInfo>()));
            expander.ExpandedLines().Should().Equal(new string[]
            {
            "class Program{",
            "static Main(){",
            "Console.WriteLine(2);",
            "}",
            "}",
            });
            switch (Environment.NewLine)
            {
                case "\n":
                    expander.ExpandedString().Should().Be("class Program{\nstatic Main(){\nConsole.WriteLine(2);\n}\n}");
                    break;
                case "\r\n":
                    expander.ExpandedString().Should().Be("class Program{\r\nstatic Main(){\r\nConsole.WriteLine(2);\r\n}\r\n}");
                    break;
            }
        }

        private class TestExpander : Expander
        {
            public TestExpander(string code, SourceFileContainer sourceFileContainer)
                : base(code, sourceFileContainer) { }
            public override IEnumerable<string> ExpandedLines() => new string[]
            {
            "class Program{",
            "static Main(){",
            "Console.WriteLine(2);",
            "}",
            "}",
            };
        }
    }
}
