using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace SourceExpander.Embedder.Test
{
    public class CompilationExtensionTest
    {
        public static TheoryData ResolveCommomPrefixTestData = new TheoryData<string[], string>
        {
            {
                new []{"", "foo"},
                ""
            },
            {
                new []{"f", "foo"},
                "f"
            },
            {
                new []{"/mnt/c/source/test/Foo.cs", "/mnt/c/source/test/Bar.cs"},
                "/mnt/c/source/test/"
            },
            {
                new []{"../source/test/Foo.cs", "../source/test/Bar.cs"},
                "../source/test/"
            },
            {
                new []{"/home/Foo.cs", "/mnt/c/source/test/Bar.cs"},
                "/"
            },
        };

        [Theory]
        [MemberData(nameof(ResolveCommomPrefixTestData))]
        public void ResolveCommomPrefixTest(IEnumerable<string> strs, string expected)
            => CompilationExtension.ResolveCommomPrefix(strs).Should().Be(expected);

    }
}
