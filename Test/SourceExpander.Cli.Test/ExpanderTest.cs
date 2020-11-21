using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
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
            var expander = new TestExpander();
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
            public TestExpander() : base(new SourceFileContainer(Array.Empty<SourceFileInfo>())) { }
            public override IEnumerable<string> ExpandedLines() => new string[]
            {
            "class Program{",
            "static Main(){",
            "Console.WriteLine(2);",
            "}",
            "}",
            };
        }


        [Fact]
        public void LoadEmbedded()
        {
            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new InvalidOperationException();

            var sourceFileInfos = Expander.ParseEmbeddedJson(Assembly.LoadFile(Path.Combine(dir, "testdata", "SampleLibrary.Old.dll")));
            sourceFileInfos.Select(s => s.FileName)
                .Should()
                .BeEquivalentTo("_SampleLibrary>Bit.cs", "_SampleLibrary>Put.cs", "_SampleLibrary>Xorshift.cs");

            var sourceFileInfos2 = Expander.ParseEmbeddedJson(Assembly.LoadFile(Path.Combine(dir, "testdata", "SampleLibrary2.dll")));
            sourceFileInfos2.Select(s => s.FileName)
                .Should()
                .BeEquivalentTo("_SampleLibrary2>Put2.cs");
        }
    }
}
