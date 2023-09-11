using AtCoder;

namespace SampleLibrary
{
    public partial class UnionFind : Dsu
    {
        public UnionFind(int n) : base(n) { Foo(); }

        void Foo() => Bar();
        partial void Bar();
    }
    [SourceExpander.NotEmbeddingSource]
    partial class UnionFind
    {
        partial void Bar() { }
    }
}
