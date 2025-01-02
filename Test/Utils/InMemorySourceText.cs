using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace SourceExpander
{
    public class InMemorySourceText(string path, string source, Encoding encoding) : AdditionalText
    {
        public InMemorySourceText(string path, string source) : this(path, source, new UTF8Encoding(false)) { }

        public override string Path { get; } = path;
        private readonly SourceText sourceText = SourceText.From(source, encoding);
        public override SourceText GetText(CancellationToken cancellationToken = default) => sourceText;
        public static implicit operator (string filename, SourceText content)(InMemorySourceText at)
            => (at.Path, at.sourceText);
    }
}
