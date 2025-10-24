using System.Collections.Generic;

namespace SourceExpander
{
    public class EmbeddingResolverTest
    {
        public static TheoryData<string[], string> ResolveCommomPrefixTestData = new()
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
            {
                new []{"/home/Foo.cs"},
                "/home/"
            },
            {
                new []{"/Foo.cs"},
                "/"
            },
        };

        [Theory]
        [MemberData(nameof(ResolveCommomPrefixTestData))]
        public void ResolveCommomPrefixTest(IEnumerable<string> strs, string expected)
            => EmbeddingResolver.ResolveCommomPrefix(strs).ShouldBe(expected);
    }
}
