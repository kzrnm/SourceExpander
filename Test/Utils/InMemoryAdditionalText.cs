using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace SourceExpander
{
    internal class InMemoryAdditionalText : AdditionalText
    {
        public InMemoryAdditionalText(string path, string source) : this(path, source, new UTF8Encoding(false)) { }
        public InMemoryAdditionalText(string path, string source, Encoding encoding)
        {
            Path = path;
            sourceText = SourceText.From(source, encoding);
        }
        public override string Path { get; }
        private readonly SourceText sourceText;
        public override SourceText GetText(CancellationToken cancellationToken = default) => sourceText;
    }
}
