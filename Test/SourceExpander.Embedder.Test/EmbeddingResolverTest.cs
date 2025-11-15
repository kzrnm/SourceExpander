using System;
using System.Collections.Generic;

namespace SourceExpander
{
    public class EmbeddingResolverTest
    {
        public static IEnumerable<Func<(string[], string)>> ResolveCommomPrefixTestData()
        {
            yield return () => (
                ["", "foo"],
                ""
            );
            yield return () => (
                ["f", "foo"],
                "f"
            );
            yield return () => (
                ["/mnt/c/source/test/Foo.cs", "/mnt/c/source/test/Bar.cs"],
                "/mnt/c/source/test/"
            );
            yield return () => (
                ["../source/test/Foo.cs", "../source/test/Bar.cs"],
                "../source/test/"
            );
            yield return () => (
                ["/home/Foo.cs", "/mnt/c/source/test/Bar.cs"],
                "/"
            );
            yield return () => (
                ["/home/Foo.cs"],
                "/home/"
            );
            yield return () => (
                ["/Foo.cs"],
                "/"
            );
        }

        [Test]
        [MethodDataSource(nameof(ResolveCommomPrefixTestData))]
        public void ResolveCommomPrefixTest(IEnumerable<string> strs, string expected)
            => EmbeddingResolver.ResolveCommomPrefix(strs).ShouldBe(expected);
    }
}
