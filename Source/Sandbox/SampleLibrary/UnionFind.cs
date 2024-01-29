using System.Diagnostics.CodeAnalysis;
using AtCoder;

namespace SampleLibrary
{
    public partial class UnionFind : Dsu
    {
        public UnionFind(int n) : base(n) { Foo(); }

        void Foo() => Bar();
        public bool Try([NotNullWhen(true)] out string? text)
        {
            if (this.Size(0) == 1)
            {
                text = "Single";
                return true;
            }
            text = null;
            return false;
        }
        partial void Bar();
    }
    [SourceExpander.NotEmbeddingSource]
    partial class UnionFind
    {
        partial void Bar() { }
    }
}
